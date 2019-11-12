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
            this.label1.Text = "current turn number: " + CURRENT_TURN;

            MethodInvoker inv = delegate
            {
                int currentTurnNumber = memoryReader.getCurrentTurnNumber();
                bool isIngameMenuActive = memoryReader.isIngameMenuActive();
                bool isActive = memoryReader.isWh2ProcessWindowActive();

                // TODO: fix me, detect if game is still in on campaign map
                memoryReader.resetAddressCurrentTurnNumber();

                if (currentTurnNumber == (CURRENT_TURN + 1))
                {
                    Debug.WriteLine("[WH2_TURN_START_NOTIFIER] new turn started");
                    CURRENT_TURN = currentTurnNumber;
                    if (!isActive) System.Media.SystemSounds.Beep.Play();

                    // only update label when number is incremented by 1
                    this.label1.Text = "current turn number: " + currentTurnNumber;
                }
                else if (currentTurnNumber == CURRENT_TURN && isIngameMenuActive)
                {
                    Debug.WriteLine("[WH2_TURN_START_NOTIFIER] action is required!");
                    if (!isActive) System.Media.SystemSounds.Asterisk.Play();
                }

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
    }
}
