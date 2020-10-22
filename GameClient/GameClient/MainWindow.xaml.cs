using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
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
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;
        
        string colorTeamOne = "#FFFFFFFF";
        string colorTeamTwo = "#FF000000";
        static bool whatTeam = true;

        string isUseButt = "";
        List<Shashka> teamOne = new List<Shashka>();
        List<Shashka> teamTwo = new List<Shashka>();
        UIElementCollection item;

        //ств ліст батонів для змешнення коду в подальшій праці з ними
        List<Button> listButtons;
        Shashka tempBut = new Shashka();
        Shashka obj = new Shashka();

        static Shashka tempEnimyBE = new Shashka();
        static Shashka tempEnimyAF = new Shashka();
        static Shashka tempEnimyBEBlack = new Shashka();
        static Shashka tempEnimyAFBlack = new Shashka();

        static string tempEnimyDELET ="";
        static string message="";
        public MainWindow()
        {
            client = new TcpClient();
            try
            {
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream(); // получаем поток
                // запускаем новый поток для получения данных
                Thread receiveThread = new Thread(new ThreadStart(ReceiveData));
                receiveThread.Start(); //старт потока
            }
            catch (Exception ex)
            {
            
            }
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
        void ReceiveData()
        {
            while (true)
            {
                //  try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.UTF8.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                     message = builder.ToString();
                        int x = Convert.ToInt32(message[0].ToString());
                        int y = Convert.ToInt32(message[1].ToString());
                        int z = Convert.ToInt32(message[2].ToString());
                        int a = Convert.ToInt32(message[3].ToString());
                  //  if(message.Length==6)
                  //      tempEnimyDELET = message[4].ToString() + message[5].ToString();

                    if (whatTeam) {
                        tempEnimyBEBlack.X = x;
                        tempEnimyBEBlack.Y = y;
                        tempEnimyAFBlack.X = z;
                        tempEnimyAFBlack.Y = a;
                        if (message.Length == 6)
                        {
                            //Dispatcher.Invoke(()=>teamTwo.Remove(teamTwo.Where(i => i.X == Convert.ToInt32(message[4].ToString()) && i.Y == Convert.ToInt32(message[5].ToString())).FirstOrDefault()));
                            //Dispatcher.Invoke(() =>FindMtBut(0,0,false).Content=null);
                            Dispatcher.Invoke(() => EnemiTurn(whatTeam, 6, Convert.ToInt32(message[4].ToString()), Convert.ToInt32(message[5].ToString())));
                        }
                        else
                        Dispatcher.Invoke(()=> EnemiTurn(whatTeam,4,0,0));

                    }
                    else
                    {
                        tempEnimyBE.X = x;
                        tempEnimyBE.Y = y;
                        tempEnimyAF.X = z;
                        tempEnimyAF.Y = a;
                        if (message.Length == 6)
                        {
                            //Dispatcher.Invoke(()=>teamOne.Remove(teamOne.Where(i => i.X == Convert.ToInt32(message[4].ToString()) && i.Y == Convert.ToInt32(message[5].ToString())).FirstOrDefault()));
                            //Dispatcher.Invoke(() => FindMtBut(0, 0, true).Content = null);
                            Dispatcher.Invoke(() => EnemiTurn(whatTeam, 6, Convert.ToInt32(message[4].ToString()), Convert.ToInt32(message[5].ToString())));
                        }
                        else
                            Dispatcher.Invoke(() => EnemiTurn(whatTeam,4,0,0));
                    }
                    tempEnimyDELET = "";
                }
            }
        //    catch { Disconnect(); }
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
                if (tempEnimyBEBlack.X != 100 && tempEnimyBEBlack.Y != 100 && tempEnimyAFBlack.X != 100 && tempEnimyAFBlack.Y != 100)
                    Turn(teamOne, button, colorTeamTwo, teamTwo);
                else
                    Title = "Waite Black turn";
            else if (!whatTeam)
                if (tempEnimyBE.X != 100 && tempEnimyBE.Y != 100 && tempEnimyAF.X != 100 && tempEnimyAF.Y != 100)
                    Turn(teamTwo, button, colorTeamOne, teamOne);
                else
                    Title = "Waite White turn";

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
                     SendData();
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
                               // ForStart(FindMtBut(1, 1, whatTeam), team);
                                FindMtBut(1, 1, whatTeam).Content = null;
                                FindMtBut(0, 0, whatTeam).BorderBrush = null;
                                FindMtBut(0, 0, whatTeam).BorderThickness = new Thickness(0.0);
                                FindMtBut(0, 0, whatTeam).Tag = "unuse";

                                tempEnimyDELET = FindMtBut(1, 1, whatTeam).Name[1].ToString() + FindMtBut(1, 1, whatTeam).Name[2].ToString();
                                Enemi.Remove(Enemi.Where(x=>x.X == Convert.ToInt32(FindMtBut(1, 1, whatTeam).Name[1].ToString()) && x.Y== Convert.ToInt32(FindMtBut(1, 1, whatTeam).Name[2].ToString())).FirstOrDefault());
                                FindMtBut(2, 2, whatTeam).Content = FindMtBut(0, 0, whatTeam).Content;

                                tempBut.X = Convert.ToInt32(button.Name[1].ToString());
                                tempBut.Y = Convert.ToInt32(button.Name[2].ToString());
                                if (whatTeam)
                                {
                                    SendData();

                                    Title = "White go";
                                    tempEnimyBEBlack.X = 100;
                                    tempEnimyBEBlack.Y = 100;
                                    tempEnimyAFBlack.X = 100;
                                    tempEnimyAFBlack.Y = 100;
                                }
                                else
                                {
                                    SendData();

                                    Title = "Black go";
                                    tempEnimyBE.X = 100;
                                    tempEnimyBE.Y = 100;
                                    tempEnimyAF.X = 100;
                                    tempEnimyAF.Y = 100;
                                }
                                //listButtons.Where(x => Umova(x, 1, 1, whatTeam)).FirstOrDefault().Content = null;
                                //  Enemi.Remove(Enemi.Where(x => x.X == Convert.ToInt32(listButtons.Where(u => Umova(u, 1, 1, whatTeam)).FirstOrDefault().Name[1].ToString()) && x.Y == Convert.ToInt32(listButtons.Where(u => Umova(u, 1, 1, whatTeam)).FirstOrDefault().Name[2].ToString())).FirstOrDefault());
                            }
                            else if (Umova(button, 2, -2, whatTeam)
                            && button.Content == null)
                            {
                                // ForStart(FindMtBut(1, -1, whatTeam), team);
                                FindMtBut(1, -1, whatTeam).Content = null;
                                FindMtBut(0, 0, whatTeam).BorderBrush = null;
                                FindMtBut(0, 0, whatTeam).BorderThickness = new Thickness(0.0);
                                FindMtBut(0, 0, whatTeam).Tag = "unuse";

                                tempEnimyDELET = FindMtBut(1, -1, whatTeam).Name[1].ToString() + FindMtBut(1, -1, whatTeam).Name[2].ToString();
                                Enemi.Remove(Enemi.Where(x => x.X == Convert.ToInt32(FindMtBut(1, -1, whatTeam).Name[1].ToString()) && x.Y == Convert.ToInt32(FindMtBut(1, -1, whatTeam).Name[2].ToString())).FirstOrDefault());
                                FindMtBut(2, -2, whatTeam).Content = FindMtBut(0, 0, whatTeam).Content;

                                tempBut.X = Convert.ToInt32(button.Name[1].ToString());
                                tempBut.Y = Convert.ToInt32(button.Name[2].ToString());
                                isUseButt = "";
                                if (whatTeam)
                                {
                                    SendData();
                                   
                                    Title = "White go";
                                    tempEnimyBEBlack.X = 100;
                                    tempEnimyBEBlack.Y = 100;
                                    tempEnimyAFBlack.X = 100;
                                    tempEnimyAFBlack.Y = 100;
                                }
                                else
                                {
                                    SendData();
                              
                                    Title = "Black go";
                                    tempEnimyBE.X = 100;
                                    tempEnimyBE.Y = 100;
                                    tempEnimyAF.X = 100;
                                    tempEnimyAF.Y = 100;
                                }
                               // listButtons.Where(x => Umova(x, 1, -1, whatTeam)).FirstOrDefault().Content = null;
                                // Enemi.Remove(Enemi.Where(x => x.X == Convert.ToInt32(listButtons.Where(u => Umova(u, 1, -1, whatTeam)).FirstOrDefault().Name[1].ToString()) && x.Y == Convert.ToInt32(listButtons.Where(u => Umova(u, 1, -1, whatTeam)).FirstOrDefault().Name[2].ToString())).FirstOrDefault());
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

            Button button = new Button() { Content = new Ellipse() { Stroke = (Brush)bc.ConvertFrom("#A9A9A9") } };
            return button;
        }
        BrushConverter bc = new BrushConverter();
        Shashka shashka1 = new Shashka();
        Shashka shashka2 = new Shashka();
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
                {
                    SendData();
                    //if (message.Length==4 )
                    //{ 
                    //listButtons.Where(j => j.Name == "P" + (tempEnimyBEBlack.X).ToString() + (tempEnimyBEBlack.Y).ToString()).FirstOrDefault().Content = null;
                    //listButtons.Where(j => j.Name == "P" + (tempEnimyAFBlack.X).ToString() + (tempEnimyAFBlack.Y).ToString()).FirstOrDefault().Content = new Ellipse() { Stroke = (Brush)bc.ConvertFrom("#FFFFFFFF") };
                    //    shashka1 = teamTwo.Where(x => x.X == tempEnimyBEBlack.X && x.Y == tempEnimyBEBlack.Y).FirstOrDefault();
                    //    shashka1.X = tempEnimyAFBlack.X;
                    //    shashka1.Y = tempEnimyAFBlack.Y;
                    //    //teamTwo.Where(x => x.X == tempEnimyBEBlack.X && x.Y == tempEnimyBEBlack.Y).FirstOrDefault().X = tempEnimyAFBlack.X;
                    //    //teamTwo.Where(x => x.X == tempEnimyBEBlack.X && x.Y == tempEnimyBEBlack.Y).FirstOrDefault().Y = tempEnimyAFBlack.Y;
                    //    RefreshXY(teamOne);
                    //RefreshXY(teamTwo);
                    //message = "";
                    //}
                        Title = "White go";
                        tempEnimyBEBlack.X = 100;
                        tempEnimyBEBlack.Y = 100;
                        tempEnimyAFBlack.X = 100;
                        tempEnimyAFBlack.Y = 100;


                }
                else
                {
                        SendData();
                    //if (message.Length == 4 )
                    //{
                    //    listButtons.Where(j => j.Name == "P" + (tempEnimyBE.X).ToString() + (tempEnimyBE.Y).ToString()).FirstOrDefault().Content = null;
                    //    listButtons.Where(j => j.Name == "P" + (tempEnimyAF.X).ToString() + (tempEnimyAF.Y).ToString()).FirstOrDefault().Content = new Ellipse() { Stroke = (Brush)bc.ConvertFrom("#FF000000") };
                    //    shashka2 = teamOne.Where(x => x.X == tempEnimyBE.X && x.Y == tempEnimyBE.Y).FirstOrDefault();
                    //    shashka2.X = tempEnimyAF.X;
                    //    shashka2.Y = tempEnimyAF.Y;
                    //    //teamOne.Where(x => x.X == tempEnimyBE.X && x.Y == tempEnimyBE.Y).FirstOrDefault().X = tempEnimyAF.X;
                    //    //teamOne.Where(x => x.X == tempEnimyBE.X && x.Y == tempEnimyBE.Y).FirstOrDefault().Y = tempEnimyAF.Y;
                    //    RefreshXY(teamOne);
                    //    RefreshXY(teamTwo);
                    //    message = "";
                    //}
                        Title = "Black go";
                        tempEnimyBE.X = 100;
                        tempEnimyBE.Y = 100;
                        tempEnimyAF.X = 100;
                        tempEnimyAF.Y = 100;
                }
            }
            catch { }
        }
        void EnemiTurn(bool whatteam,int count,int posX, int posY)
        {
            if (whatteam) {
                listButtons.Where(j => j.Name == "P" + (tempEnimyBEBlack.X).ToString() + (tempEnimyBEBlack.Y).ToString()).FirstOrDefault().Content = null;
                listButtons.Where(j => j.Name == "P" + (tempEnimyAFBlack.X).ToString() + (tempEnimyAFBlack.Y).ToString()).FirstOrDefault().Content = new Ellipse() { Stroke = (Brush)bc.ConvertFrom("#FFFFFFFF") };
                shashka1 = teamTwo.Where(x => x.X == tempEnimyBEBlack.X && x.Y == tempEnimyBEBlack.Y).FirstOrDefault();
                shashka1.X = tempEnimyAFBlack.X;
                shashka1.Y = tempEnimyAFBlack.Y;

                if (count == 6)
                {
                    listButtons.Where(j => j.Name == "P" + posX.ToString() + posY.ToString()).FirstOrDefault().Content = null;
                    listButtons.Where(j => j.Name == "P" + posX.ToString() + posY.ToString()).FirstOrDefault().Tag = "unuse";
                    teamOne.Remove(teamOne.Where(x => x.X == posX && x.Y == posY).FirstOrDefault());
                }

                //teamTwo.Where(x => x.X == tempEnimyBEBlack.X && x.Y == tempEnimyBEBlack.Y).FirstOrDefault().X = tempEnimyAFBlack.X;
                //teamTwo.Where(x => x.X == tempEnimyBEBlack.X && x.Y == tempEnimyBEBlack.Y).FirstOrDefault().Y = tempEnimyAFBlack.Y;
                RefreshXY(teamOne);
                RefreshXY(teamTwo);

               // tempEnimyDELET = "";
            }
            else {
                listButtons.Where(j => j.Name == "P" + (tempEnimyBE.X).ToString() + (tempEnimyBE.Y).ToString()).FirstOrDefault().Content = null;
                listButtons.Where(j => j.Name == "P" + (tempEnimyAF.X).ToString() + (tempEnimyAF.Y).ToString()).FirstOrDefault().Content = new Ellipse() { Stroke = (Brush)bc.ConvertFrom("#FF000000") };
                shashka2 = teamOne.Where(x => x.X == tempEnimyBE.X && x.Y == tempEnimyBE.Y).FirstOrDefault();
                shashka2.X = tempEnimyAF.X;
                shashka2.Y = tempEnimyAF.Y;

                if (count == 6)
                {
                    listButtons.Where(j => j.Name == "P" + posX.ToString() + posY.ToString()).FirstOrDefault().Content = null;
                    listButtons.Where(j => j.Name == "P" + posX.ToString() + posY.ToString()).FirstOrDefault().Tag = "unuse";
                    teamOne.Remove(teamOne.Where(x => x.X == posX && x.Y == posY).FirstOrDefault());
                }

                //teamOne.Where(x => x.X == tempEnimyBE.X && x.Y == tempEnimyBE.Y).FirstOrDefault().X = tempEnimyAF.X;
                //teamOne.Where(x => x.X == tempEnimyBE.X && x.Y == tempEnimyBE.Y).FirstOrDefault().Y = tempEnimyAF.Y;
                RefreshXY(teamOne);
                RefreshXY(teamTwo);

               // tempEnimyDELET = "";
            }
        }
        string cord = "";
        private void SendData()
        {

            cord += tempBut.X.ToString() + tempBut.Y.ToString() +tempEnimyDELET; //////cord
            if (cord.Length==4 || cord.Length == 6)
            {
                byte[] data = Encoding.Unicode.GetBytes(cord);
                stream.Write(data, 0, data.Length);
                cord = "";
                tempEnimyDELET = "";
            }
        }
        void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }

        private void Button_Click_white(object sender, RoutedEventArgs e)
        {
            whatTeam = true;
        }

        private void Button_Click_black(object sender, RoutedEventArgs e)
        {
            whatTeam = false;
        
        }
    }
}
