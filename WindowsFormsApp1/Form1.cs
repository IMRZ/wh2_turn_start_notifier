using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private MemoryReader memoryReader;

        public Form1()
        {
            InitializeComponent();
            Start();
        }

        private void Start()
        {
            memoryReader = new MemoryReader();

            int CURRENT_TURN = memoryReader.getCurrentTurnNumber();

            MethodInvoker inv = delegate
            {
                int currentTurnNumber = memoryReader.getCurrentTurnNumber();
                bool isIngameMenuActive = memoryReader.isIngameMenuActive();
                bool isActive = memoryReader.isWh2ProcessWindowActive();

                if (currentTurnNumber > CURRENT_TURN)
                {
                    Debug.WriteLine("[WH2_TURN_START_NOTIFIER] new turn started");
                    CURRENT_TURN = currentTurnNumber;
                    if (!isActive) System.Media.SystemSounds.Beep.Play();
                }
                else if (currentTurnNumber == CURRENT_TURN && isIngameMenuActive)
                {
                    Debug.WriteLine("[WH2_TURN_START_NOTIFIER] action is required!");
                    if (!isActive) System.Media.SystemSounds.Asterisk.Play();
                }

                this.label1.Text = "current turn number: " + currentTurnNumber;
                this.label2.Text = "player action required: " + isIngameMenuActive;
            };

            Task.Run(() => {
                while (true)
                {
                    try
                    {
                        this.Invoke(inv);
                        System.Threading.Thread.Sleep(1000);
                    } catch
                    {
                        // do nothing, application is stopping
                    }
                }
            });
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            memoryReader.resetAddressCurrentTurnNumber();
        }
    }
}
