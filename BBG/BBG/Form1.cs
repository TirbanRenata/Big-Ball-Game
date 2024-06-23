using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BBG
{
    public partial class Form1 : Form
    {
        // Numărul constant de bile
        private const int BallCount = 15;
        // Lista pentru a stoca bilele
        private List<Ball> balls = new List<Ball>();
        // Generator de numere aleatoare
        private Random random = new Random();
        // Indicator pentru a ști dacă simularea s-a terminat
        bool finished;
        // Pensulă pentru desenarea bilelor (nefolosită momentan)
        private Brush brush = new SolidBrush(Color.Black);

        public Form1()
        {
            InitializeComponent();
            // Inițializarea bilelor la crearea formularului
            InitializeBalls();
        }

        // Funcție pentru a inițializa bilele
        private void InitializeBalls()
        {
            for (int i = 0; i < BallCount; i++)
            {
                // Alegerea unui tip de bilă aleatoriu
                BallType type = (BallType)random.Next(0, 3);
                // Stabilirea unei raze aleatorii
                int radius = random.Next(10, 20);
                // Stabilirea unei culori aleatorii
                Color color = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                // Stabilirea unei poziții aleatorii pe canvas
                int x = random.Next(radius * 2 + 30, canvas.Width - radius * 2 - 30);
                int y = random.Next(radius * 2 + 30, canvas.Height - radius * 2 - 30);
                // Stabilirea vitezei aleatorii
                int dx = random.Next(-4, 5);
                int dy = random.Next(-4, 5);

                // Adăugarea bilei în lista de bile
                balls.Add(new Ball(radius, color, new Point(x, y), dx, dy, type));
            }
        }

        // Funcție pentru a desena bilele pe canvas
        private void DrawBalls(Graphics g)
        {

            foreach (Ball ball in balls)
            {
                // Crearea unei pensule cu culoarea bilei
                Brush brush = new SolidBrush(ball.Color);
                // Desenarea bilei pe canvas
                g.FillEllipse(brush, ball.Position.X, ball.Position.Y, ball.Radius * 2, ball.Radius * 2);
                // Eliberarea resurselor pensulei
                brush.Dispose();

                // Desenarea tipului bilei
                string ballTypeText = ball.Type.ToString();
                Font font = new Font("Arial", 10);
                Brush textBrush = new SolidBrush(Color.Black);
                g.DrawString(ballTypeText, font, textBrush, ball.Position.X, ball.Position.Y - 15);
                font.Dispose();
                textBrush.Dispose();
            }
        }

        // Funcție pentru a actualiza starea bilelor într-o tură
        private void Turn()
        {
            List<Tuple<Ball, Ball>> processedCollisions = new List<Tuple<Ball, Ball>>();

            foreach (Ball ball in balls)
            {
                ball.Move(canvas.ClientRectangle);

                foreach (Ball other in balls)
                {
                    if (ball != other && ball.Intersects(other))
                    {
                        // Verificăm dacă această pereche de bile a fost deja procesată
                        if (!processedCollisions.Contains(Tuple.Create(ball, other)) &&
                            !processedCollisions.Contains(Tuple.Create(other, ball)))
                        {
                            // Gestionarea coliziunii între bile
                            ball.Collide(other);
                            // Adăugăm perechea de bile în lista de coliziuni procesate
                            processedCollisions.Add(Tuple.Create(ball, other));
                        }
                    }
                }
            }

            balls.RemoveAll(ball => ball.Type == BallType.Regular && ball.Radius <= 0);
            if (balls.FindAll(ball => ball.Type == BallType.Regular).Count == 0)
            {
                finished = true;
            }
        }

        // Evenimentul de încărcare al formularului
        private void Form1_Load(object sender, EventArgs e)
        {
            // Pornirea timerului pentru a actualiza starea bilelor periodic
            timer.Start();
        }

        // Evenimentul de desenare al canvasului
        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            // Desenarea bilelor pe canvas
            DrawBalls(e.Graphics);
        }

        // Evenimentul de tic-tac al timerului
        private void timer_Tick(object sender, EventArgs e)
        {
            if (!finished)
            {
                // Actualizarea stării bilelor într-o tură
                Turn();
                // Reîmprospătarea canvasului pentru a reflecta modificările
                canvas.Refresh();
            }
            else
            {
                // Oprirea timerului dacă simularea s-a terminat
                timer.Stop();
            }
        }
    }

    // Enumerarea tipurilor de bile
    public enum BallType
    {
        Regular,
        Monster,
        Repellent
    }

    // Clasa Ball care reprezintă o bilă
    public class Ball
    {
        // Proprietăți ale bilei
        public int Radius { get; set; }
        public Color Color { get; set; }
        public Point Position { get; set; }
        public int Dx { get; set; }
        public int Dy { get; set; }
        public BallType Type { get; set; }

        public Ball(int radius, Color color, Point position, int dx, int dy, BallType type)
        {
            Radius = radius;
            Color = color;
            Position = position;
            Dx = dx;
            Dy = dy;
            Type = type;
            // Dacă bila este de tip Monster, viteza este zero
            if (Type == BallType.Monster)
            {
                dx = 0;
                dy = 0;
            }
        }

        // Funcție pentru a muta bila pe canvas
        public void Move(Rectangle bounds)
        {
            Position = new Point(Position.X + Dx, Position.Y + Dy);

            // Reflexia bilei la coliziunea cu marginile canvasului
            if (Position.X <= bounds.Left || Position.X + Radius * 2 >= bounds.Right)
            {
                Dx = -Dx; // Reflexie pe axa X
            }

            if (Position.Y <= bounds.Top || Position.Y + Radius * 2 >= bounds.Bottom)
            {
                Dy = -Dy; // Reflexie pe axa Y
            }
        }

        // Funcție pentru a verifica coliziunea cu o altă bilă
        public bool Intersects(Ball other)
        {
            int distance = (int)Math.Sqrt(Math.Pow(Position.X - other.Position.X, 2) + Math.Pow(Position.Y - other.Position.Y, 2));
            return distance <= Radius + other.Radius;
        }

        // Funcție pentru a gestiona coliziunea între bile
        public void Collide(Ball other)
        {
            switch (Type)
            {
                case BallType.Regular:
                    switch (other.Type)
                    {
                        case BallType.Regular:
                            if (Radius > other.Radius)
                            {
                                Radius += other.Radius;
                                Color = CombineColors(Color, other.Color, other.Radius);
                                other.Radius = 0; // Marca bila cealaltă ca fiind dispărută
                            }
                            else
                            {
                                other.Radius += Radius;
                                other.Color = CombineColors(Color, other.Color, Radius);
                                Radius = 0; // Marca bila curentă ca fiind dispărută
                            }
                            break;
                        case BallType.Monster:
                            other.Radius += Radius;
                            Radius = 0; // Marca bila curentă ca fiind dispărută
                            break;
                        case BallType.Repellent:
                            // Schimbăm culoarea doar o singură dată
                            if (Color != other.Color)
                            {
                                Color = other.Color;
                            }
                            Dx = -Dx; // Schimbare direcție
                            break;
                    }
                    break;
                case BallType.Repellent:
                    switch (other.Type)
                    {
                        case BallType.Repellent:
                            // Schimbăm culoarea doar o singură dată
                            if (Color != other.Color)
                            {
                                Color temp = Color;
                                Color = other.Color;
                                other.Color = temp;
                            }
                            break;
                        case BallType.Monster:
                            Radius /= 2;
                            break;
                    }
                    break;
                case BallType.Monster:
                    break;
            }
        }


        // Funcție pentru a combina culorile a două bile
        private Color CombineColors(Color color1, Color color2, int weight)
        {
            if (weight == 0)
                weight = 1;
            int r = (color1.R * weight + color2.R * (weight * 2)) / (weight * 3);
            int g = (color1.G * weight + color2.G * (weight * 2)) / (weight * 3);
            int b = (color1.B * weight + color2.B * (weight * 2)) / (weight * 3);
            return Color.FromArgb(r, g, b);
        }
    }
}
