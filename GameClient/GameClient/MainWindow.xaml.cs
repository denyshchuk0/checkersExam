using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
        //для зєднання
        private const string host = "127.0.0.1";
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;

        List<Shashka> team = new List<Shashka>();
        List<Shashka> enemi = new List<Shashka>();
        List<Shashka> teamWhite = new List<Shashka>();
        List<Shashka> teamBlack = new List<Shashka>();
        List<Button> listButtons;

        BrushConverter brush_convert = new BrushConverter();

        UIElementCollection item;

        string coordinate_sending = "";

        string block_sending = "1";

        string colorTeamOne = "#FFFFFFFF";
        string colorTeamTwo = "#FF000000";
        static bool whatTeam = true;

        string isUseButt = "";

        //ств ліст батонів для змешнення коду в подальшій праці з ними
        Shashka tempBut = new Shashka();
        Shashka obj = new Shashka();

        static Shashka tempEnimyBE = new Shashka();
        static Shashka tempEnimyAF = new Shashka();
        static Shashka tempEnimyBEBlack = new Shashka();
        static Shashka tempEnimyAFBlack = new Shashka();

        static string tempEnimyDELET = "";
        static string message = "";
        public MainWindow()
        {
            InitializeComponent();

            listButtons = new List<Button>();
            item = this.Doska.Children;

            // Connect();
            CreateDoska();
        }
        #region Work with Server
        private void Connect()
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
                MessageBox.Show(ex.Message);
            }
        }
        private void SendData()
        {
            coordinate_sending += tempBut.X.ToString() + tempBut.Y.ToString() + tempEnimyDELET + block_sending; //////cord
            if (coordinate_sending.Length == 5 || coordinate_sending.Length == 7)
            {
                byte[] data = Encoding.Unicode.GetBytes(coordinate_sending);
                stream.Write(data, 0, data.Length);
                coordinate_sending = "";
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
        #endregion
        private void CreateDoska()
        {
            int u = 0;
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                    if (y % 2 == 0 && x % 2 == 0 || y % 2 != 0 && x % 2 != 0 && (item[u] is Button))
                    {
                        Button button = (item[u] as Button);
                        listButtons.Add(button);
                        button.Name = "P" + y.ToString() + x.ToString();
                        button.Tag = "unuse";
                        button.Click += Button_Click;
                        if (whatTeam)
                            if (x <= 2)
                                teamWhite.Add(new Shashka(colorTeamOne) { X = x, Y = y });
                            else if (x >= 5)
                                teamBlack.Add(new Shashka(colorTeamTwo) { X = x, Y = y });
                            u++;
                    }
            RefreshXY(teamWhite);
            RefreshXY(teamBlack);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (sender as Button);
            if (whatTeam)
                if (block_sending == "0")
                    Turn( button, colorTeamTwo);
                else
                    Screen.Content = "Waite Black turn";
            else if (!whatTeam)
                if (block_sending == "0")
                    Turn( button, colorTeamOne);
                else
                    Screen.Content = "Waite White turn";
        }
        #region Turn
        void Turn( Button button, string colorEnemi)
        {
            //перевіряємо чи є шашка на кнопці або чи вибрана яка інша шашка на полі 
            if (button.Content is Ellipse || listButtons.Find(x => Umova(x, 0, 0, whatTeam)).BorderBrush == Brushes.Red)
            {
                //перевірка чи це перша шашка або чи це та вже вибрана шашка
                if (isUseButt == button.Name || isUseButt == "" && (button.Content as Ellipse).Stroke.ToString() != colorEnemi)
                {
                    //обираємо шашку якщо вона НЕ вибрана
                    if (button.Tag.ToString() == "unuse")
                    {
                        button.BorderBrush = Brushes.Red;
                        button.BorderThickness = new Thickness(3.0);
                        button.Tag = "use";
                        isUseButt = button.Name;
                        tempBut.X = Convert.ToInt32(button.Name[1].ToString());
                        tempBut.Y = Convert.ToInt32(button.Name[2].ToString());
                    }
                    //якщо вибрана то знімаємо рамку 
                    else
                    {
                        CanselBorder(button);
                        isUseButt = "";
                    }
                }
                //перевіряємо чи там не має іншої шашки + перевіряємо чи бажаний хід є в зоні обмеження 
                else
                {
                   
                    if (Umova(button, 1, 1, whatTeam) && button.Content == null || Umova(button, 1, -1, whatTeam) && button.Content == null)
                    {
                        coordinate_sending = tempBut.X.ToString() + tempBut.Y.ToString();
                        //повертаємо попередній кнопці тег "не використовується" і обнуляємо
                        CanselBorder(listButtons.Where(x => Umova(x, 0, 0, whatTeam)).FirstOrDefault());
                        listButtons.Where(x => Umova(x, 0, 0, whatTeam)).FirstOrDefault().Content = null;

                        obj = team.Where(x => x.X == tempBut.X && x.Y == tempBut.Y).FirstOrDefault();

                        button.Content = obj.form;
                        obj.X = Convert.ToInt32(button.Name[1].ToString());
                        obj.Y = Convert.ToInt32(button.Name[2].ToString());
                        tempBut.X = obj.X;
                        tempBut.Y = obj.Y;

                        isUseButt = "";
                        if (whatTeam)
                            Block("Waite Black turn", "1", "0");
                        else
                            Block("Waite White turn", "1", "0");
                    }
                    else
                    {
                        string swTemp = "";
                        swTemp = (Convert.ToInt32(button.Name[1].ToString()) - tempBut.X).ToString();
                        swTemp += (Convert.ToInt32(button.Name[2].ToString()) - tempBut.Y).ToString();
                        switch (swTemp)
                        {
                            case "22":
                                TurnAtack( button, colorEnemi,  1, 1, 2, 2);
                                break;
                            case "2-2":
                                TurnAtack( button, colorEnemi,  1, -1, 2, -2);
                                break;
                            case "-22":
                                TurnAtack( button, colorEnemi,  -1, 1, -2, 2);
                                break;
                            case "-2-2":
                                TurnAtack( button, colorEnemi,  -1, -1, -2, -2);
                                break;
                            default:
                                break;
                        }
                        swTemp = "";
                    }
                }
                RefreshXY(team);
                if (team.Count == 0)
                {
                    MessageBox.Show("LOOOSSEERRRR!!!");
                 //   CreateDoska();
                }
                else if (enemi.Count == 0)
                {
                    MessageBox.Show("WInnneeerrr!!");
                  //  CreateDoska();
                }
            }
        }
        private void TurnAtack( Button button, string colorEnemi, int xEnemi, int yEnemi, int xTurn, int yTurn)
        {

            if (FindMtBut(xEnemi, yEnemi).Content != null && (FindMtBut(xEnemi, yEnemi).Content as Ellipse).Stroke.ToString() == colorEnemi && button.Content == null)
            {
                coordinate_sending = tempBut.X.ToString() + tempBut.Y.ToString();
                FindMtBut(xEnemi, yEnemi).Content = null;
                CanselBorder(FindMtBut(0, 0));

                tempEnimyDELET = FindMtBut(xEnemi, yEnemi).Name[1].ToString() + FindMtBut(xEnemi, yEnemi).Name[2].ToString();
                enemi.Remove(enemi.Where(x => x.X == Convert.ToInt32(FindMtBut(xEnemi, yEnemi).Name[1].ToString()) && x.Y == Convert.ToInt32(FindMtBut(xEnemi, yEnemi).Name[2].ToString())).FirstOrDefault());

                obj = team.Where(x => x.X == tempBut.X && x.Y == tempBut.Y).FirstOrDefault();
                obj.X = Convert.ToInt32(button.Name[1].ToString());
                obj.Y = Convert.ToInt32(button.Name[2].ToString());

                FindMtBut(xTurn, yTurn).Content = FindMtBut(0, 0).Content;

                FindMtBut(0, 0).Content = null;

                tempBut.X = Convert.ToInt32(button.Name[1].ToString());
                tempBut.Y = Convert.ToInt32(button.Name[2].ToString());
                isUseButt = "";
                if (UmovaRamka(1, 1, 2, 2, colorEnemi, '7', '7')
                || UmovaRamka(1, -1, 2, -2, colorEnemi, '7', '0')
                || UmovaRamka(-1, 1, -2, 2, colorEnemi, '0', '7')
                || UmovaRamka(-1, -1, -2, -2, colorEnemi, '0', '0'))
                {
                    button.BorderBrush = Brushes.Red;
                    button.BorderThickness = new Thickness(3.0);
                    button.Tag = "use";
                    isUseButt = button.Name;
                    Block("You can attacking enemi", "0", "1");
                }
                else
                {
                    if (whatTeam)
                        Block("Waite Black turn", "1", "0");
                    else
                        Block("Waite White turn", "1", "0");
                }
            }
        }
        #endregion

        #region Service Funk
        bool UmovaRamka(int x1, int y1, int x2, int y2, string colorEnemi, char umX, char umY)
        {
            if (FindMtBut(x1, y1).Name[1] != umX && FindMtBut(x1, y1).Name[2] != umY)
                if (FindMtBut(x1, y1).Content != null && (FindMtBut(x1, y1).Content as Ellipse).Stroke.ToString() == colorEnemi && FindMtBut(x2, y2).Content == null && UmovaFild(x2, y2))
                    return true;
            return false;
        }
        private void RefreshXY(List<Shashka> shashkas)
        {
            for (int i = 0; i < shashkas.Count; i++)
                foreach (var ite in item)
                    if (ite is Button)
                    {
                        Button button = (ite as Button);
                        if (button.Name == "P" + shashkas[i].X.ToString() + shashkas[i].Y.ToString())
                        {
                            button.Content = shashkas[i].form;
                            break;
                        }
                    }
        }

        private void Block(string status, string white, string black)
        {
            block_sending = black;
            SendData();
            Screen.Content = status;
            block_sending = white;
        }

        private static void CanselBorder(Button button)
        {
            button.BorderBrush = null;
            button.BorderThickness = new Thickness(0.0);
            button.Tag = "unuse";
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
            else if (WhoseTurn==false && tempBut.Y - y < 8 && tempBut.Y - y >= 0 && tempBut.X - x < 8 && tempBut.X - x >= 0)
            {
                if (button.Name == "P" + (tempBut.X - x).ToString() + (tempBut.Y - y).ToString())
                    return true;
                else
                    return false;
            }
            return false;
        }
        bool UmovaFild(int x, int y)
        {
            if (tempBut.Y + y < 8 && tempBut.Y + y >= 0 && tempBut.X + x < 8 && tempBut.X + x >= 0)
                return true;
            return false;
        }
        Button FindMtBut(int x, int y)
        {
            if (tempBut.Y + y < 8 && tempBut.Y + y >= 0 && tempBut.X + x < 8 && tempBut.X + x >= 0)
                return listButtons.Where(j => j.Name == "P" + (tempBut.X + x).ToString() + (tempBut.Y + y).ToString()).FirstOrDefault();
           return listButtons.Where(j => j.Name == "P" + (tempBut.X).ToString() + (tempBut.Y).ToString()).FirstOrDefault();
        }
        #endregion
        #region Enemi Turn
        void ReceiveData()
        {
            while (true)
            {
                try
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
                    if (message.Length == 5)
                        block_sending = message[4].ToString();
                    else
                        block_sending = message[6].ToString();

                    if (whatTeam)
                    {
                        tempEnimyBEBlack.X = x;
                        tempEnimyBEBlack.Y = y;
                        tempEnimyAFBlack.X = z;
                        tempEnimyAFBlack.Y = a;
                        if (message.Length == 7)
                            Dispatcher.Invoke(() => EnemiTurn(whatTeam, 7, Convert.ToInt32(message[4].ToString()), Convert.ToInt32(message[5].ToString())));
                        else
                            Dispatcher.Invoke(() => EnemiTurn(whatTeam, 5, 0, 0));
                        Dispatcher.Invoke(() => Screen.Content = " White is going");
                    }
                    else
                    {
                        tempEnimyBE.X = x;
                        tempEnimyBE.Y = y;
                        tempEnimyAF.X = z;
                        tempEnimyAF.Y = a;
                        if (message.Length == 7)
                            Dispatcher.Invoke(() => EnemiTurn(whatTeam, 7, Convert.ToInt32(message[4].ToString()), Convert.ToInt32(message[5].ToString())));
                        else
                            Dispatcher.Invoke(() => EnemiTurn(whatTeam, 5, 0, 0));
                        Dispatcher.Invoke(() => Screen.Content = " Black is going");
                    }
                    tempEnimyDELET = "";
                   
                }
                catch (Exception ex)
                { MessageBox.Show(ex.Message); Disconnect(); }
            }
        }
        void EnemiTurn(bool whatteam, int count, int posX, int posY)
        {
            if (whatteam)
                ShowEnemi(count, posX, posY, tempEnimyBEBlack, tempEnimyAFBlack, colorTeamTwo);
            else
                ShowEnemi(count, posX, posY, tempEnimyBE, tempEnimyAF,  colorTeamOne);

            RefreshXY(team);
            RefreshXY(enemi);
        }
        private void ShowEnemi(int count, int posX, int posY, Shashka shashkaBE, Shashka shashkaAF, string color)
        {
            Shashka shashka = new Shashka();

            listButtons.Where(j => j.Name == "P" + (shashkaBE.X).ToString() + (shashkaBE.Y).ToString()).FirstOrDefault().Content = null;
            listButtons.Where(j => j.Name == "P" + (shashkaAF.X).ToString() + (shashkaAF.Y).ToString()).FirstOrDefault().Content = new Ellipse() { Stroke = (Brush)brush_convert.ConvertFrom(color) };
            shashka = enemi.Where(x => x.X == shashkaBE.X && x.Y == shashkaBE.Y).FirstOrDefault();
            shashka.X = shashkaAF.X;
            shashka.Y = shashkaAF.Y;
            if (count == 7)
            {
                listButtons.Where(j => j.Name == "P" + posX.ToString() + posY.ToString()).FirstOrDefault().Content = null;
                listButtons.Where(j => j.Name == "P" + posX.ToString() + posY.ToString()).FirstOrDefault().Tag = "unuse";
                team.Remove(team.Where(x => x.X == posX && x.Y == posY).FirstOrDefault());
            }
        }
        #endregion

        #region Meny
        private void Button_Click_white(object sender, RoutedEventArgs e)
        {
            block_sending = "0";
            whatTeam = true;
            Doska.Visibility = Visibility.Visible;
            SelectTeam.Visibility = Visibility.Hidden;
            brRamka.Visibility = Visibility.Visible;
            Screen.Visibility = Visibility.Visible;
            
            team = teamWhite;
            enemi = teamBlack;

        }
        private void Button_Click_black(object sender, RoutedEventArgs e)
        {
            whatTeam = false;
            Doska.Visibility = Visibility.Visible;
            SelectTeam.Visibility = Visibility.Hidden;
            brRamka.Visibility = Visibility.Visible;
            Screen.Visibility = Visibility.Visible;
          
            team = teamBlack;
            enemi = teamWhite;
        }

        private void StartClick(object sender, RoutedEventArgs e)
        {
            Connect();
            Menu.Visibility = Visibility.Hidden;
            SelectTeam.Visibility = Visibility.Visible;
        }
        private void ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Disconnect();
        }
    }
}
