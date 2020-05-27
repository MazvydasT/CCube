using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CCube
{
    public class LogInGroupData
    {
        public string Title { get; private set; }
        public string GroupName { get; private set; }

        public LogInGroupData(string title, string groupName)
        {
            Title = title;
            GroupName = groupName;
        }
    }

    /// <summary>
    /// Interaction logic for LogIn.xaml
    /// </summary>
    public partial class LogIn : UserControl
    {
        public LogInGroupData[] LogInGroupDataSet { get; } = new[]
        {
            new LogInGroupData("eMS", "ems"),
            new LogInGroupData("Teamcenter", "tce")
        };

        private Control eMSLogInGroup = null;
        private Control EMSLogInGroup
        {
            get
            {
                if (eMSLogInGroup == null)
                    eMSLogInGroup = Utils.FindVisualChildren<Control>(this).Where(child => Equals(child.Tag, "ems")).FirstOrDefault();

                return eMSLogInGroup;
            }
        }

        private Control tceLogInGroup = null;
        private Control TCeLogInGroup
        {
            get
            {
                if (tceLogInGroup == null)
                    tceLogInGroup = Utils.FindVisualChildren<Control>(this).Where(child => Equals(child.Tag, "tce")).FirstOrDefault();

                return tceLogInGroup;
            }
        }

        private TextBox eMSUserNameTextBox = null;
        private TextBox EMSUserNameTextBox
        {
            get
            {
                if (eMSUserNameTextBox == null)
                    eMSUserNameTextBox = (TextBox)LogicalTreeHelper.FindLogicalNode(EMSLogInGroup, "UserName");

                return eMSUserNameTextBox;
            }
        }

        private TextBox tceUserNameTextBox = null;
        private TextBox TCeUserNameTextBox
        {
            get
            {
                if (tceUserNameTextBox == null)
                    tceUserNameTextBox = (TextBox)LogicalTreeHelper.FindLogicalNode(TCeLogInGroup, "UserName");

                return tceUserNameTextBox;
            }
        }

        private PasswordBox eMSPasswordBox = null;
        private PasswordBox EMSPasswordBox
        {
            get
            {
                if (eMSPasswordBox == null)
                    eMSPasswordBox = (PasswordBox)LogicalTreeHelper.FindLogicalNode(EMSLogInGroup, "Password");

                return eMSPasswordBox;
            }
        }

        private PasswordBox tcePasswordBox = null;
        private PasswordBox TCePasswordBox
        {
            get
            {
                if (tcePasswordBox == null)
                    tcePasswordBox = (PasswordBox)LogicalTreeHelper.FindLogicalNode(TCeLogInGroup, "Password");

                return tcePasswordBox;
            }
        }

        private TextBox eMSPasswordVisibleTextBox = null;
        private TextBox EMSPasswordVisibleTextBox
        {
            get
            {
                if (eMSPasswordVisibleTextBox == null)
                    eMSPasswordVisibleTextBox = (TextBox)LogicalTreeHelper.FindLogicalNode(EMSLogInGroup, "PasswordVisible");

                return eMSPasswordVisibleTextBox;
            }
        }

        private TextBox tcePasswordVisibleTextBox = null;
        private TextBox TCePasswordVisibleTextBox
        {
            get
            {
                if (tcePasswordVisibleTextBox == null)
                    tcePasswordVisibleTextBox = (TextBox)LogicalTreeHelper.FindLogicalNode(TCeLogInGroup, "PasswordVisible");

                return tcePasswordVisibleTextBox;
            }
        }

        /*private Button eMSShowPasswordButton = null;
        private Button EMSShowPasswordButton
        {
            get
            {
                if (eMSShowPasswordButton == null)
                    eMSShowPasswordButton = (Button)LogicalTreeHelper.FindLogicalNode(EMSLogInGroup, "ShowPassword");

                return eMSShowPasswordButton;
            }
        }*/

        private Button tceShowPasswordButton = null;
        private Button TCeShowPasswordButton
        {
            get
            {
                if (tceShowPasswordButton == null)
                    tceShowPasswordButton = (Button)LogicalTreeHelper.FindLogicalNode(TCeLogInGroup, "ShowPassword");

                return tceShowPasswordButton;
            }
        }

        public string EMSUserName => EMSUserNameTextBox.Text;
        public SecureString EMSPassword => EMSPasswordBox.SecurePassword;

        public string TCeUserName => TCeUserNameTextBox.Text;
        public SecureString TCePassword => TCePasswordBox.SecurePassword;

        public LogIn()
        {
            InitializeComponent();
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tceButtonPressed = Equals(sender, TCeShowPasswordButton);

            var passwordVisible = tceButtonPressed ? TCePasswordVisibleTextBox : EMSPasswordVisibleTextBox;
            var password = tceButtonPressed ? TCePasswordBox : EMSPasswordBox;

            passwordVisible.Text = password.Password;
            password.Visibility = Visibility.Hidden;
            passwordVisible.Visibility = Visibility.Visible;
        }

        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var tceButtonPressed = Equals(sender, TCeShowPasswordButton);

            var passwordVisible = tceButtonPressed ? TCePasswordVisibleTextBox : EMSPasswordVisibleTextBox;
            var password = tceButtonPressed ? TCePasswordBox : EMSPasswordBox;

            passwordVisible.Text = null;
            password.Visibility = Visibility.Visible; ;
            passwordVisible.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}