using CsvHelper;
using CsvHelper.Configuration;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace Plot_Those_Lines
{
    /*
     * Extensions :
     * CSVReader
     * Scottplot (Windows Forms)
    */
    public partial class Form1 : Form
    {
        //va directement sur la bonne directory
        //au lieu de devoire faire C:/Documents/GitHub..etc
        private string csvFilePath = Path.Combine(Application.StartupPath, "data.csv");

        public Form1()
        {
            InitializeComponent();

            //charge le CSV initial s'il existe
            if (File.Exists(csvFilePath))
                LoadCsvAndPlot(csvFilePath);
            else
                MessageBox.Show("Le fichier data.csv n'existe pas.");
        }

        private void LoadCsvAndPlot(string path)
        {
            try
            {
                //StreamReader = classe derive de TextReader
                using (var reader = new StreamReader(path))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    //CultureInfo.InvariantCulture pour format universel 
                    csv.Read();
                    csv.ReadHeader();
                    var headers = csv.HeaderRecord;

                    var years = new List<double>();
                    var data = new Dictionary<string, List<double>>();

                    foreach (var header in headers.Skip(1)) //premiere collone = Annee donc saute
                    {
                        //nouvelle liste vide apart la premiere
                        data[header] = new List<double>();
                    }

                    while (csv.Read())
                    {
                        double yearVal;
                        if (double.TryParse(csv.GetField(headers[0]), out yearVal))
                        {
                            years.Add(yearVal);
                        }
                        else
                        {
                            //si pas de if else tout pete TODO fix ca
                            years.Add(double.NaN);
                        }

                        //puts data into dictionary
                        foreach (var pos in data.Keys.ToList())
                        {
                            double val;
                            if (double.TryParse(csv.GetField(pos), out val))
                            {
                                data[pos].Add(val);
                            }
                            else
                            {
                                data[pos].Add(double.NaN);
                            }
                        }
                    }

                    //put list onto array
                    double[] dataX = years.ToArray();

                    //efface le graphe precedent
                    formsPlot1.Plot.Clear();

                    foreach (var key in data)
                    {
                        double[] dataY = key.Value.ToArray();
                        var scatter = formsPlot1.Plot.Add.Scatter(dataX, dataY);
                        s
                        //shows all teams index
                        scatter.LegendText = key.Key;
                    }

                    //TODO: make this be custom title by user input
                    formsPlot1.Plot.Title("NBA teams wins per year");

                    //TODO: make this get data from CSV 
                    formsPlot1.Plot.XLabel("Year");
                    formsPlot1.Plot.YLabel("Wins");

                    /*
                     * TODO: figure out how to do this
                     * formsPlot1.Axes.SetLimits(2000, 2025, 0, 82);
                     */
                    formsPlot1.Plot.Legend.IsVisible = true;

                    formsPlot1.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement du CSV : " + ex.Message);
            }
            //dont remove future me it will do terrible things c# will not be happy
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //dont remove future me it will do terrible things c# will not be happy
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"C:\";
                openFileDialog.Filter = "CSV files (*.csv)|*.csv";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFile = openFileDialog.FileName;

                        //force copie le fichier
                        File.Copy(selectedFile, csvFilePath, true);

                        //charge et affiche le nouveau fichier CSV
                        LoadCsvAndPlot(csvFilePath);
                }
            }
        }
        
    }
}