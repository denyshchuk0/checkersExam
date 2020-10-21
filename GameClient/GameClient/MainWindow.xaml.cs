using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string colorTeamOne = "#FFFFFFFF";
        string colorTeamTwo = "#FF000000";
        bool whatTeam = true;

        string isUseButt = "";
        List<Shashka> teamOne = new List<Shashka>();
        List<Shashka> teamTwo = new List<Shashka>();
        UIElementCollection item;
        //ств ліст батонів для змешнення коду в подальшій праці з ними
        List<Button> listButtons;
        Shashka tempBut = new Shashka();
        Shashka obj = new Shashka();
        public MainWindow()
        {
            InitializeComponent();
            item = this.Doska.Children;
            listButtons = new List<Button>();
            var bc = new BrushConverter();
            int u = 0;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (y % 2 == 0 && x % 2 == 0 || y % 2 != 0 && x % 2 != 0 && (item[u] is Button))
                    {
                        Button button = (item[u] as Button);
                        listButtons.Add(button);
                        button.Name = "P" + y.ToString() + x.ToString();
                        button.Tag = "unuse";
                        button.Click += Button_Click;
                        if (x <= 2)
                            teamOne.Add(new Shashka(colorTeamOne) { X = x, Y = y });
                        else if (x >= 5)
                            teamTwo.Add(new Shashka(colorTeamTwo) { X = x, Y = y });
                        u++;
                    }
                }
            }

            RefreshXY(teamOne);
            RefreshXY(teamTwo);
        }
        private void RefreshXY(List<Shashka> shashkas)
        {
            for (int i = 0; i < shashkas.Count; i++)
            {
                foreach (var ite in this.Doska.Children)
                {
                    if (ite is Button)
                    {
                        Button button = (ite as Button);
                        if (button.Name == "P" + shashkas[i].X.ToString() + shashkas[i].Y.ToString())
                            button.Content = shashkas[i].form;
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (sender as Button);
            if (whatTeam)
                Turn(teamOne, button, colorTeamTwo, teamTwo);
            else if (!whatTeam)
                Turn(teamTwo, button, colorTeamOne, teamOne);
        }

        void Turn(List<Shashka> team, Button button, string colorEnemi, List<Shashka> Enemi)
        {
            //перевіряємо чи є шашка на кнопці або чи вибрана яка інша шашка на полі 
            if (button.Content is Ellipse || listButtons.Find(x => Umova(x, 0, 0, whatTeam)).BorderBrush == Brushes.Red)
            {
                //перевірка чи це перша шашка або чи це та вже вибрана шашка
                if (isUseButt == button.Name || isUseButt == "")
                {
                    //обираємо шашку якщо вона НЕ вибрана
                    if (button.Tag.ToString() == "unuse")
                    {
                        button.BorderBrush = Brushes.Red;
                        button.BorderThickness = new Thickness(10.0);
                        button.Tag = "use";
                        isUseButt = button.Name;
                        tempBut.X = Convert.ToInt32(button.Name[1].ToString());
                        tempBut.Y = Convert.ToInt32(button.Name[2].ToString());

                    }
                    //якщо вибрана то знімаємо рамку 
                    else
                    {
                        button.BorderBrush = null;
                        button.BorderThickness = new Thickness(0.0);
                        button.Tag = "unuse";
                        isUseButt = "";
                    }
                }
                //перевіряємо чи там не має іншої шашки + перевіряємо чи бажаний хід є в зоні обмеження 
                else
                {
                    if (Umova(button, 1, 1, whatTeam) && button.Content == null || Umova(button, 1, -1, whatTeam) && button.Content == null)
                    {
                        //повертаємо попередній кнопці тег "не використовується" і обнуляємо
                        ForStart(button, team);

                    }
                    else if (FindMtBut(1, 1, whatTeam).Content != null && (FindMtBut(1, 1, whatTeam).Content as Ellipse).Stroke.ToString() == colorEnemi
                      || FindMtBut(1, -1, whatTeam).Content != null && (FindMtBut(1, -1, whatTeam).Content as Ellipse).Stroke.ToString() == colorEnemi)
                    {
                        if (Umova(button, 2, 2, whatTeam) || Umova(button, 2, -2, whatTeam))
                        {

                            if (Umova(button, 2, 2, whatTeam)
                               && button.Content == null)
                            {
                                ForStart(button, team);

                                listButtons.Where(x => Umova(x, 1, 1, whatTeam)).FirstOrDefault().Content = null;
                                Enemi.Remove(Enemi.Where(x => x.X == Convert.ToInt32(listButtons.Where(u => Umova(u, 1, 1, whatTeam)).FirstOrDefault().Name[1].ToString()) && x.Y == Convert.ToInt32(listButtons.Where(u => Umova(u, 1, 1, whatTeam)).FirstOrDefault().Name[2].ToString())).FirstOrDefault());
                            }
                            else if (Umova(button, 2, -2, whatTeam)
                            && button.Content == null)
                            {
                                ForStart(button, team);

                                listButtons.Where(x => Umova(x, 1, -1, whatTeam)).FirstOrDefault().Content = null;
                                Enemi.Remove(Enemi.Where(x => x.X == Convert.ToInt32(listButtons.Where(u => Umova(u, 1, -1, whatTeam)).FirstOrDefault().Name[1].ToString()) && x.Y == Convert.ToInt32(listButtons.Where(u => Umova(u, 1, -1, whatTeam)).FirstOrDefault().Name[2].ToString())).FirstOrDefault());
                            }
                        }
                    }
                }
                RefreshXY(team);
            }
        }
        bool Umova(Button button, int x, int y, bool WhoseTurn)
        {
            if (WhoseTurn && tempBut.Y + y < 8 && tempBut.Y + y >= 0 && tempBut.X + x < 8 && tempBut.X + x >= 0)
            {
                if (button.Name == "P" + (tempBut.X + x).ToString() + (tempBut.Y + y).ToString())
                    return true;
                else
                    return false;
            }
            else if (tempBut.Y - y < 8 && tempBut.Y - y >= 0 && tempBut.X - x < 8 && tempBut.X - x >= 0)
            {
                if (button.Name == "P" + (tempBut.X - x).ToString() + (tempBut.Y - y).ToString())
                    return true;
                else
                    return false;
            }
            return false;
        }
        Button FindMtBut(int x, int y, bool WhoseTurn)
        {
            if (WhoseTurn && tempBut.Y + y < 8 && tempBut.Y + y >= 0 && tempBut.X + x < 8 && tempBut.X + x >= 0)
                return listButtons.Where(j => j.Name == "P" + (tempBut.X + x).ToString() + (tempBut.Y + y).ToString()).FirstOrDefault();
            else if (tempBut.Y - y < 8 && tempBut.Y - y >= 0 && tempBut.X - x < 8 && tempBut.X - x >= 0)
                return listButtons.Where(j => j.Name == "P" + (tempBut.X - x).ToString() + (tempBut.Y - y).ToString()).FirstOrDefault();

            var bc = new BrushConverter();
            Button button = new Button() { Content = new Ellipse() { Stroke = (Brush)bc.ConvertFrom("#A9A9A9") } };
            return button;
        }
        void ForStart(Button button, List<Shashka> team)
        {
            try
            {
                listButtons.Where(x => Umova(x, 0, 0, whatTeam)).FirstOrDefault().Tag = "unuse";
                listButtons.Where(x => Umova(x, 0, 0, whatTeam)).FirstOrDefault().BorderBrush = null;
                listButtons.Where(x => Umova(x, 0, 0, whatTeam)).FirstOrDefault().BorderThickness = new Thickness(0.0);
                listButtons.Where(x => Umova(x, 0, 0, whatTeam)).FirstOrDefault().Content = null;

                obj = team.Where(x => x.X == tempBut.X && x.Y == tempBut.Y).FirstOrDefault();

                button.Content = obj.form;
                obj.X = Convert.ToInt32(button.Name[1].ToString());
                obj.Y = Convert.ToInt32(button.Name[2].ToString());
                tempBut.X = obj.X;
                tempBut.Y = obj.Y;

                isUseButt = "";
                if (whatTeam)
                    whatTeam = false;
                else
                    whatTeam = true;
            }
            catch { }
        }
    }
}
