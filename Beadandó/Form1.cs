using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Beadandó
{
    public partial class Form1 : Form
    {
        MySqlConnection conn;
        public Form1()
        {
            InitializeComponent();
            conn = new MySqlConnection("Server=localhost; DataBase=cs_harcosok; Uid=root; Pwd=");
            try
            {
                conn.Open();
                Adatszerkezet();

            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Nem tudott kapcsolodni");
                Application.Exit();
            }
        }

        private void Adatszerkezet()
        {
            var command = conn.CreateCommand();
            command.CommandText = @"
                                CREATE TABLE IF NOT EXISTS harcosok(
                                    id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                    nev VARCHAR(80) NOT NULL UNIQUE,
                                    letrehozas DATE NOT NULL
                                );
                                CREATE TABLE IF NOT EXISTS kepessegek(
                                        id INTEGER PRIMARY KEY AUTO_INCREMENT,
                                        nev VARCHAR(80) NOT NULL,
                                        leiras VARCHAR(1000) NOT NULL,
                                        harcos_id INTEGER NOT NULL,
                                        FOREIGN KEY(harcos_id) REFERENCES harcosok(id)
                                );
                                ";
            command.ExecuteNonQuery();

            var lekerdezesCommand = conn.CreateCommand();
            lekerdezesCommand.CommandText = @"select nev, letrehozas from harcosok";
            using (var reader = lekerdezesCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    string nev = reader.GetString("nev");
                    DateTime datum = reader.GetDateTime("letrehozas");
                    string sor = string.Format("{0} {1:yyyy. MM. dd.}", nev, datum);
                    comboBox1.Items.Add(nev);
                    harcosokListBox.Items.Add(sor);
                }
            }


        }

        private void letrehozasButton_Click(object sender, EventArgs e)
        {
            string nev = harcosNevTextBox.Text;
            DateTime datum = DateTime.Now;

            var lekerdezesCommand = conn.CreateCommand();
            lekerdezesCommand.CommandText = @"select count(*) from harcosok where nev=@nev";
            lekerdezesCommand.Parameters.AddWithValue("@nev", nev);
            long db = (long)lekerdezesCommand.ExecuteScalar();
            if (db > 0)
            {
                MessageBox.Show("Ilyen harcos mar van");
                return;
            }

            var command = conn.CreateCommand();
            command.CommandText = @"INSERT INTO harcosok (nev, letrehozas) 
                                VALUES (@nev, @datum)";
            command.Parameters.AddWithValue("@nev", nev);
            command.Parameters.AddWithValue("@datum", datum);
            command.ExecuteNonQuery();

            comboBox1.Items.Add(nev);
            harcosokListBox.Items.Add(nev);
            harcosNevTextBox.Text = "";
        }

        private void hozzaadButton_Click(object sender, EventArgs e)
        {
            string nev = comboBox1.GetItemText(comboBox1.SelectedItem);
            int harcos_id = 0;
            var lekerdezesCommand = conn.CreateCommand();
            lekerdezesCommand.CommandText = @"select id from harcosok where nev=@nev";
            lekerdezesCommand.Parameters.AddWithValue("@nev", nev);
            using (var reader = lekerdezesCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32("id");
                    harcos_id = id;
                }
            }
            string kepessegNev = kepessegTextBox.Text;
            string leiras = leirasTextBox.Text;
            var command = conn.CreateCommand();
            command.CommandText = @"INSERT INTO kepessegek (nev, leiras, harcos_id) 
                                VALUES (@nev, @leiras, @harcos_id)";
            command.Parameters.AddWithValue("@nev", kepessegNev);
            command.Parameters.AddWithValue("@leiras", leiras);
            command.Parameters.AddWithValue("@harcos_id", harcos_id);
            command.ExecuteNonQuery();
            kepessegTextBox.Text = "";
            leirasTextBox.Text = "";
        }

        private void harcosokListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            kepessegListBox.Items.Clear();
            kepessegLeirasTextBox.Text = "";
            int index = (harcosokListBox.SelectedIndex) + 1;
            var lekerdezesCommand = conn.CreateCommand();
            lekerdezesCommand.CommandText = @"SELECT kepessegek.nev FROM `kepessegek` inner join harcosok on harcosok.id=kepessegek.harcos_id where harcosok.id=@index";
            lekerdezesCommand.Parameters.AddWithValue("@index", index);

            using (var reader = lekerdezesCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    string nev = reader.GetString("nev");
                    kepessegListBox.Items.Add(nev);
                }
            }

        }

        private void torlesButton_Click(object sender, EventArgs e)
        {
            try
            {
                int id = 0;
                string kivalasztott = kepessegListBox.SelectedItem.ToString();
                var lekerdezesCommand = conn.CreateCommand();
                lekerdezesCommand.CommandText = @"SELECT id FROM `kepessegek` where nev=@nev";
                lekerdezesCommand.Parameters.AddWithValue("@nev", kivalasztott);
                using (var reader = lekerdezesCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id = reader.GetInt32("id");
                    }
                }
                lekerdezesCommand.CommandText = @"DELETE FROM kepessegek WHERE id = @id";
                lekerdezesCommand.Parameters.AddWithValue("@id", id);
                lekerdezesCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nem válsztottad ki a képességet");
            }


        }

        private void kepessegListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string leiras = "";
            string kivalasztott = kepessegListBox.SelectedItem.ToString();
            var lekerdezesCommand = conn.CreateCommand();
            lekerdezesCommand.CommandText = @"SELECT leiras FROM `kepessegek` where nev=@nev";
            lekerdezesCommand.Parameters.AddWithValue("@nev", kivalasztott);
            using (var reader = lekerdezesCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    leiras = reader.GetString("leiras");
                }
            }
            kepessegLeirasTextBox.Text = leiras;
        }

        private void modositButton_Click(object sender, EventArgs e)
        {
            try
            {
                int id = 0;
                string kivalasztott = kepessegListBox.SelectedItem.ToString();
                string leiras = kepessegLeirasTextBox.Text;
                var lekerdezesCommand = conn.CreateCommand();
                lekerdezesCommand.CommandText = @"SELECT id FROM `kepessegek` where nev=@nev";
                lekerdezesCommand.Parameters.AddWithValue("@nev", kivalasztott);
                using (var reader = lekerdezesCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id = reader.GetInt32("id");
                    }
                }
                var command = conn.CreateCommand();
                command.CommandText = @"UPDATE kepessegek SET leiras = @leiras WHERE id = @id";
                command.Parameters.AddWithValue("@leiras", leiras);
                command.Parameters.AddWithValue("@id", id);
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nem válsztottad ki a képességet");
            }

        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            conn.Close();
        }
    }
}
