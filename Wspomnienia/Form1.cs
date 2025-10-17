using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Timers;

namespace MemoryGame
{
    public partial class Form1 : Form
    {
        private const int GRID_SIZE = 4;        
        private const int TOTAL_CARDS = 16;     
        private const int CARD_PAIRS = 8;       
        private const int CARD_WIDTH = 100;     
        private const int CARD_HEIGHT = 100;    
        private const int CARD_MARGIN = 10;    

        private List<PictureBox> cards;        
        private List<Image> cardImages;        
        private List<int> cardValues;          
        private PictureBox firstCard;       
        private PictureBox secondCard;      
        private bool canClick = true;      

        private int score = 0;             
        private int pairsFound = 0;       
        private DateTime gameStartTime;      
        private System.Timers.Timer gameTimer; 
        private TimeSpan elapsedTime;          

        public Form1()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            cards = new List<PictureBox>();
            cardImages = new List<Image>();
            cardValues = new List<int>();

            score = 0;
            pairsFound = 0;
            firstCard = null;
            secondCard = null;
            canClick = true;
            gameStartTime = DateTime.Now;

            lblScore.Text = $"Punkty: {score}";
            lblTime.Text = "Czas: 00:00";
            lblStatus.Text = "Gra rozpoczêta!";

            InitializeTimer();

            LoadCardImages();

            CreateGameBoard();

            ShuffleCards();
        }

        private void InitializeTimer()
        {
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer.Dispose();
            }

            gameTimer = new System.Timers.Timer(1000);
            gameTimer.Elapsed += UpdateTimer;
            gameTimer.AutoReset = true;
            gameTimer.Start();
        }

        private void UpdateTimer(object sender, ElapsedEventArgs e)
        {
            elapsedTime = DateTime.Now - gameStartTime;
            UpdateTimeLabel();
        }

        private void UpdateTimeLabel()
        {
            if (lblTime.InvokeRequired)
            {
                lblTime.Invoke(new Action(UpdateTimeLabel));
                return;
            }

            lblTime.Text = $"Czas: {elapsedTime:mm\\:ss}";
        }
        private void LoadCardImages()
        {
            cardImages.Clear();

            Color[] colors = {
                Color.Red, Color.Blue, Color.Green, Color.Yellow,
                Color.Orange, Color.Purple, Color.Pink, Color.Brown
            };

            for (int i = 0; i < CARD_PAIRS; i++)
            {
                Bitmap bmp = new Bitmap(CARD_WIDTH, CARD_HEIGHT);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    using (Brush brush = new SolidBrush(colors[i]))
                    {
                        switch (i % 4)
                        {
                            case 0:
                                g.FillEllipse(brush, 10, 10, 80, 80);
                                break;
                            case 1:
                                g.FillRectangle(brush, 10, 10, 80, 80);
                                break;
                            case 2:
                                Point[] triangle = { new Point(50, 10), new Point(10, 90), new Point(90, 90) };
                                g.FillPolygon(brush, triangle);
                                break;
                            case 3:
                                g.FillEllipse(brush, 25, 25, 50, 50);
                                break;
                        }
                    }
                }
                cardImages.Add(bmp);
            }
        }

        private void CreateGameBoard()
        {
            foreach (var card in cards)
            {
                this.Controls.Remove(card);
            }
            cards.Clear();

            for (int row = 0; row < GRID_SIZE; row++)
            {
                for (int col = 0; col < GRID_SIZE; col++)
                {
                    PictureBox card = new PictureBox();
                    card.Width = CARD_WIDTH;
                    card.Height = CARD_HEIGHT;
                    card.Left = CARD_MARGIN + col * (CARD_WIDTH + CARD_MARGIN);
                    card.Top = 80 + row * (CARD_HEIGHT + CARD_MARGIN);
                    card.BorderStyle = BorderStyle.FixedSingle;
                    card.SizeMode = PictureBoxSizeMode.StretchImage;

                    Bitmap backImage = new Bitmap(CARD_WIDTH, CARD_HEIGHT);
                    using (Graphics g = Graphics.FromImage(backImage))
                    {
                        g.Clear(Color.LightGray);
                        using (Pen pen = new Pen(Color.DarkGray, 3))
                        {
                            g.DrawRectangle(pen, 5, 5, CARD_WIDTH - 10, CARD_HEIGHT - 10);
                        }
                    }
                    card.Image = backImage;

                    card.Tag = -1;
                    card.Click += Card_Click;

                    cards.Add(card);
                    this.Controls.Add(card);
                    card.BringToFront();
                }
            }
        }

        private void ShuffleCards()
        {
            cardValues.Clear();
            Random rnd = new Random();

            for (int i = 0; i < CARD_PAIRS; i++)
            {
                cardValues.Add(i);
                cardValues.Add(i);
            }

            cardValues = cardValues.OrderBy(x => rnd.Next()).ToList();

            for (int i = 0; i < TOTAL_CARDS; i++)
            {
                cards[i].Tag = cardValues[i];
                cards[i].Visible = true;
                cards[i].Enabled = true;
            }
        }

        private void Card_Click(object sender, EventArgs e)
        {
            if (!canClick) return;

            PictureBox clickedCard = (PictureBox)sender;

            if (clickedCard == firstCard || clickedCard.Tag.ToString() == "matched")
                return;

            RevealCard(clickedCard);

            if (firstCard == null)
            {
                firstCard = clickedCard;
            }
            else
            {
                secondCard = clickedCard;
                canClick = false;

                if ((int)firstCard.Tag == (int)secondCard.Tag)
                {
                    HandleMatch();
                }
                else
                {
                    HandleMismatch();
                }
            }
        }

        private void RevealCard(PictureBox card)
        {
            int cardValue = (int)card.Tag;
            card.Image = cardImages[cardValue];
        }

        private void HideCard(PictureBox card)
        {
            Bitmap backImage = new Bitmap(CARD_WIDTH, CARD_HEIGHT);
            using (Graphics g = Graphics.FromImage(backImage))
            {
                g.Clear(Color.LightGray);
                using (Pen pen = new Pen(Color.DarkGray, 3))
                {
                    g.DrawRectangle(pen, 5, 5, CARD_WIDTH - 10, CARD_HEIGHT - 10);
                }
            }
            card.Image = backImage;
        }

        private void HandleMatch()
        {
            score += 10;
            pairsFound++;

            firstCard.Tag = "matched";
            secondCard.Tag = "matched";

            firstCard.Enabled = false;
            secondCard.Enabled = false;

            lblScore.Text = $"Punkty: {score}";
            lblStatus.Text = "Dobrze! Znaleziono parê!";

            if (pairsFound == CARD_PAIRS)
            {
                EndGame();
            }
            else
            {
                ResetTurn();
            }
        }

        private void HandleMismatch()
        {
            lblStatus.Text = "Nie pasuj¹! Spróbuj ponownie.";

            System.Windows.Forms.Timer delayTimer = new System.Windows.Forms.Timer();
            delayTimer.Interval = 1000;
            delayTimer.Tick += (s, args) =>
            {
                delayTimer.Stop();
                HideCard(firstCard);
                HideCard(secondCard);
                ResetTurn();
            };
            delayTimer.Start();
        }

        private void ResetTurn()
        {
            firstCard = null;
            secondCard = null;
            canClick = true;
        }

        private void EndGame()
        {
            gameTimer.Stop();
            canClick = false;

            string message = $"Gratulacje! Ukoñczy³eœ grê!\n" +
                           $"Wynik: {score} punktów\n" +
                           $"Czas: {elapsedTime:mm\\:ss}";

            MessageBox.Show(message, "Koniec gry", MessageBoxButtons.OK, MessageBoxIcon.Information);
            lblStatus.Text = "Gra zakoñczona! Kliknij Reset aby zagraæ ponownie.";
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            InitializeGame();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer.Dispose();
            }
            base.OnFormClosing(e);
        }
    }
}