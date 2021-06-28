using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;

namespace CCube
{
    public class ImportManager : BindableBase
    {
        public enum ImportStatusOptions
        {
            Idle,
            Running,
            Stopping
        }

        public enum ErrorStatusOptions
        {
            WrongAccountEMS,
            WrongAccountTCe,
            WrongLogInEMS,
            WrongLogInTCe,
            NoNetwork
        }

        public static ImportManager Service { get; } = new ImportManager();

        private ImportStatusOptions status = ImportStatusOptions.Idle;
        public ImportStatusOptions Status
        {
            get => status;
            private set => SetProperty(ref status, value);
        }

        private ErrorStatusOptions? ErrorStatus { get; set; } = null;

        private ErrorStatusOptions? logInErrorStatus = null;
        public ErrorStatusOptions? LogInErrorStatus
        {
            get => logInErrorStatus;
            private set => SetProperty(ref logInErrorStatus, value);
        }

        private bool retryFailed = true;
        public bool RetryFailed
        {
            get => retryFailed;
            set => SetProperty(ref retryFailed, value);
        }

        private int killCooldown = 0;
        public int KillCooldown
        {
            get => killCooldown;
            set => SetProperty(ref killCooldown, value);
        }

        private LogIn LogInWindow => ApplicationData.Service.MainWindow.LogInWindow;

        private int noNetworkWaitTimeInMinutes = 1;

        private Process currentProces;
        private Input currentInput;

        private ImportManager() { }

        public void Stop()
        {
            Status = ImportStatusOptions.Stopping;

            KillCooldown = 3;

            var timer = new System.Timers.Timer(1000)
            {
                AutoReset = true
            };

            timer.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                --KillCooldown;

                if (KillCooldown == 0) timer.Stop();
            };

            timer.Start();
        }

        public void Kill()
        {
            lock (this)
            {
                try
                {
                    currentProces?.Kill();
                }

                finally
                {
                    currentProces = null;
                }
            }
        }

        public Task Start(IEnumerable<Input> inputs = null)
        {
            if (Status == ImportStatusOptions.Running || Status == ImportStatusOptions.Stopping) return null;

            var emsUserName = LogInWindow.EMSUserName;
            var emsPassword = LogInWindow.EMSPassword;

            var tceUserName = LogInWindow.TCeUserName;
            var tcePassword = LogInWindow.TCePassword;

            if (string.IsNullOrWhiteSpace(emsUserName) || string.IsNullOrWhiteSpace(Utils.SecureStringToString(emsPassword)) ||
                    string.IsNullOrWhiteSpace(tceUserName) || string.IsNullOrWhiteSpace(Utils.SecureStringToString(tcePassword)))
            {
                LogInWindow.Visibility = System.Windows.Visibility.Visible;
                return null;
            }

            Status = ImportStatusOptions.Running;
            ErrorStatus = null;
            LogInErrorStatus = null;

            inputs = inputs ?? new Input[0];

            var task = new Task(() =>
            {
                foreach (var input in inputs)
                {
                    var latestIteration = input.LatestIteration;

                    if (latestIteration != null && latestIteration != input.CurrentActiveIteration)
                        latestIteration.CurrentStatus = Iteration.Status.Waiting;
                }

                var applicationDataService = ApplicationData.Service;
                var stats = applicationDataService.Stats;

                stats.Recalculate();
                Utils.GUIDispatcher.Invoke(() => { applicationDataService.InputsViewSource.View.Refresh(); });


                var inputsToBeProcessed = inputs.Where(input => input.CurrentActiveIteration.CurrentStatus == Iteration.Status.Waiting).ToArray();

                foreach (var input in inputsToBeProcessed)
                {
                    if (ErrorStatus != null || Status == ImportStatusOptions.Stopping) break;

                    input.CurrentActiveIteration.StartTime = DateTime.Now;
                    input.CurrentActiveIteration.CurrentStatus = Iteration.Status.Running;

                    Logger.Service.LogImport(input);

                    Utils.GUIDispatcher.Invoke(() => { applicationDataService.InputsViewSource.View.Refresh(); });

                    --stats.InputsWaiting;
                    ++stats.InputsRunning;

                    Process process = null;

                    var standardOuputLines = new List<string>();

                    try
                    {
                        process = Process.Start(new ProcessStartInfo()
                        {
                            FileName = ApplicationData.Service.PathToCCCommandExe,
                            Arguments = $"syncNode -EmsUser {emsUserName} -EmsPsswd {Utils.SecureStringToString(emsPassword)} -TcUser {tceUserName} -TcPsswd {Utils.SecureStringToString(tcePassword)} {input.CCCommandParameters}",
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        });


                        lock (this)
                        {
                            currentProces = process;
                            currentInput = input;
                        }

                        var standardOutput = process.StandardOutput;

                        while (!standardOutput.EndOfStream)
                        {
                            standardOuputLines.Add(standardOutput.ReadLine());
                        }

                        if (process.ExitCode == -1)
                        {
                            standardOuputLines.AddRange(new[]
                            {
                                "Exception",
                                "Import terminated by user."
                            });
                        }
                    }

                    catch (Exception e)
                    {
                        Stop();

                        standardOuputLines.AddRange(new[]
                        {
                            "Exception",
                            e.Message,
                            $"Could not start '{ApplicationData.Service.PathToCCCommandExe}'. Make sure it is installed and its directory is added to PATH system variable."
                        });
                    }

                    input.CurrentActiveIteration.ExecutionLog = standardOuputLines.ToArray();

                    if (!(process?.HasExited ?? true))
                    {
                        try
                        {
                            process?.Kill();
                        }

                        catch (Exception) { }
                    }

                    PostItemImport(input);
                }

                var stopping = Status == ImportStatusOptions.Stopping;

                Status = ImportStatusOptions.Idle;

                if (ErrorStatus != ErrorStatusOptions.NoNetwork) noNetworkWaitTimeInMinutes = 1;

                if (ErrorStatus == ErrorStatusOptions.NoNetwork)
                {
                    stats.ImportStartTime = null;

                    Thread.Sleep(new TimeSpan(0, noNetworkWaitTimeInMinutes, 0));

                    noNetworkWaitTimeInMinutes *= 2;
                    noNetworkWaitTimeInMinutes = noNetworkWaitTimeInMinutes > 10 ? 10 : noNetworkWaitTimeInMinutes;

                    stats.LatestIterationCompleteTime = null;
                    stats.ImportStartTime = DateTime.Now;

                    Start(inputs);
                }

                else if (ErrorStatus == ErrorStatusOptions.WrongAccountEMS || ErrorStatus == ErrorStatusOptions.WrongLogInEMS
                    || ErrorStatus == ErrorStatusOptions.WrongAccountTCe || ErrorStatus == ErrorStatusOptions.WrongLogInTCe)
                {
                    LogInErrorStatus = ErrorStatus;
                }

                else if (RetryFailed && !stopping)
                {
                    var inputsToRetry = inputsToBeProcessed.Where(input => input.LatestIteration != input.CurrentActiveIteration).ToArray();

                    if (inputsToRetry.Length > 0)
                    {
                        Utils.GUIDispatcher.Invoke(() => Start(inputsToRetry));
                    }
                }
            });

            task.Start();

            return task;
        }

        private void PostItemImport(Input input)
        {
            var currentIteration = input.CurrentActiveIteration;

            var exceptionIndex = Array.IndexOf(currentIteration.ExecutionLog, "Exception");

            var applicationService = ApplicationData.Service;
            var stats = applicationService.Stats;

            --stats.InputsRunning;
            ++stats.IterationsTotal;

            if (exceptionIndex < 0)
            {
                currentIteration.CurrentStatus = Iteration.Status.Succeeded;

                Utils.GUIDispatcher.Invoke(() => { applicationService.InputsViewSource.View.Refresh(); });

                ++stats.InputsSuccessful;
                ++stats.IterationsSuccessful;
                ++stats.IterationsSuccessfulSinceStart;
            }

            else
            {
                currentIteration.CurrentStatus = Iteration.Status.Failed;

                Utils.GUIDispatcher.Invoke(() => { applicationService.InputsViewSource.View.Refresh(); });

                ++stats.InputsFailed;
                ++stats.IterationsFailed;

                var message = new Regex(@"\s{2,}").Replace(string.Join(" ", input.CurrentActiveIteration.ExecutionLog.Skip(exceptionIndex + 1)).Trim(), " ");

                if (message.StartsWith("Create new sync request failed: The \"Write\" access is denied on the object"))
                {
                    ErrorStatus = ErrorStatusOptions.WrongAccountTCe;
                    message = "Wrong TCe account used.";
                    input.AddIteration();
                }

                else if (message.StartsWith("Login to AIWebService failed: TC ERROR: The login attempt failed: either the user ID or the password is invalid"))
                {
                    ErrorStatus = ErrorStatusOptions.WrongLogInTCe;
                    message = "TCe user name or password is incorrect.";
                    input.AddIteration();
                }

                else if (message.StartsWith("The Node must be checked out"))
                {
                    ErrorStatus = ErrorStatusOptions.WrongAccountEMS;
                    message = "Wrong eMS account used.";
                    input.AddIteration();
                }

                else if (message.StartsWith("The user name or password is incorrect"))
                {
                    ErrorStatus = ErrorStatusOptions.WrongLogInEMS;
                    message = "eMS user name or password is incorrect.";
                    input.AddIteration();
                }

                else if (message.StartsWith("The RPC server is unavailable"))
                {
                    ErrorStatus = ErrorStatusOptions.NoNetwork;
                    message = "Network error.";
                    input.AddIteration();
                }

                else if (message.StartsWith("You must check out"))
                {
                    message = "Object must be checked out.";
                }

                else if (message.StartsWith("Invalid Object ID"))
                {
                    message = "Invalid Project Id";
                }

                else
                {
                    input.AddIteration();
                }

                currentIteration.Message = message;
            }

            var now = DateTime.Now;

            input.CurrentActiveIteration.EndTime = now;
            stats.LatestIterationCompleteTime = now;

            Logger.Service.LogImport(input);
        }
    }
}