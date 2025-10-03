using CsvHelper;
using CsvHelper.Configuration;
using ScottPlot;
using ScottPlot.Palettes;
using ScottPlot.PlotStyles;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

        //classe pour stocker les donnees de chaque serie pour le hover
        private class SeriesData
        {
            public string Name { get; set; }
            public double[] XValues { get; set; }
            public double[] YValues { get; set; }
        }
        //liste qui garde toutes les donnees pour detection hover
        private List<SeriesData> allSeriesData = new List<SeriesData>();

        public Form1()
        {
            InitializeComponent();

            //charge le CSV initial s'il existe
            if (File.Exists(csvFilePath))
                LoadCsvAndPlot(csvFilePath);
            else
                MessageBox.Show("Le fichier data.csv n'existe pas.");

            //event pour detecter le hover sur les points
            formsPlot1.MouseMove += FormsPlot1_MouseMove;
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
                    allSeriesData.Clear();

                    //object de couleur scottplot -- contiend des coleurs tres legeres TODO: FIX
                    var palette = new Category20();


                    formsPlot1.Refresh();

                    //key doit etre = var et donc je dois declarer autre iterator
                    int i = 0;
                    foreach (var key in data)
                    {
                        double[] dataY = key.Value.ToArray();
                        var scatter = formsPlot1.Plot.Add.Scatter(dataX, dataY);

                        scatter.Color = palette.GetColor(i);

                        // label for legend
                        scatter.LegendText = key.Key;

                        allSeriesData.Add(new SeriesData
                        {
                            Name = key.Key,
                            XValues = dataX,
                            YValues = dataY
                        });

                        i++;
                    }

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
            textBox1.Text = "Enter your title here...";
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

                    //Si le fichier existe, compare les 2
                    if (File.Exists(csvFilePath) && FileCompare(selectedFile, csvFilePath))
                    {
                        MessageBox.Show("The selected file is identical to the existing data", "Error - Duplicate file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    //true pour forcer -- windows a des problemes avec la liberte assez souvent
                    File.Copy(selectedFile, csvFilePath, true);

                    //charge et affiche le nouveau fichier CSV
                    LoadCsvAndPlot(csvFilePath);
                }
            }
        }

        /*
         * Pas mon code, volé de 
         * https://learn.microsoft.com/en-us/troubleshoot/developer/visualstudio/csharp/language-compilers/create-file-compare
         */
        private bool FileCompare(string file1, string file2)
        {

            if (file1 == file2)
                return true;

            using (FileStream fs1 = new FileStream(file1, FileMode.Open))
            using (FileStream fs2 = new FileStream(file2, FileMode.Open))
            {
                //if not same length -- cannot be the same file
                if (fs1.Length != fs2.Length)
                    return false;

                int file1byte;
                int file2byte;

                //reads bytes of each until finished or mismatch
                do
                {
                    file1byte = fs1.ReadByte();
                    file2byte = fs2.ReadByte();
                }
                while ((file1byte == file2byte) && (file1byte != -1));

                //return bool
                return (file1byte == file2byte);
            }
        }

        private void formsPlot1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            string insertedTitle = textBox1.Text;
            formsPlot1.Plot.Title(insertedTitle);
            formsPlot1.Refresh();
        }
        //trouve et affiche le point le plus proche de la souris
        //ultra consomateur de memoire
        private void FormsPlot1_MouseMove(object sender, MouseEventArgs mouse)
        {
            var mouseCoord = formsPlot1.Plot.GetCoordinates(mouse.X, mouse.Y);

            double minDistance = double.MaxValue;
            double matchedX = double.NaN;
            double matchedY = double.NaN;
            List<string> hoveredTeams = new List<string>();

            //parcours toutes les series
            foreach (var series in allSeriesData)
            {
                for (int i = 0; i < series.XValues.Length; i++)
                {
                    //skip points invalides --theoriquement pas besoin par ce que c'est check
                    //a l'import mais l'import ne check pas les valeurs negatives TODO fix eventuellement
                    if (double.IsNaN(series.XValues[i]) || double.IsNaN(series.YValues[i]))
                        continue;

                    //calcule distance en pixels entre souris et point
                    var pointPixel = formsPlot1.Plot.GetPixel(new ScottPlot.Coordinates(series.XValues[i], series.YValues[i]));

                    //calcule difference avec pythogore
                    double diffx = pointPixel.X - mouse.X;
                    double diffy = pointPixel.Y - mouse.Y;
                    double distance = Math.Sqrt(diffx * diffx + diffy * diffy);

                    //si ce point est plus proche et dans les 50 pixels
                    if (distance < minDistance && distance < 50)
                    {
                        minDistance = distance;
                        matchedX = series.XValues[i];
                        matchedY = series.YValues[i];
                    }
                }
            }

            //affiche tooltip ou titre normal
            if (double.IsNaN(matchedX) || double.IsNaN(matchedY))
            {
                label1.Text = "";
                formsPlot1.Cursor = Cursors.Default;
                return;
            }

            
            hoveredTeams.Clear();
            double tolerance = 1e-6; //nesseaire par ce que 1.00001 != 1, donc comme ca c'est accurate a 1 millionth 

            foreach (var series in allSeriesData)
            {
                for (int i = 0; i < series.XValues.Length; i++)
                {
                    if (double.IsNaN(series.XValues[i]) || double.IsNaN(series.YValues[i]))
                        continue;

                    if (Math.Abs(series.XValues[i] - matchedX) < tolerance &&
                        Math.Abs(series.YValues[i] - matchedY) < tolerance)
                        //prend valeur absolue avec tolerance 
                        // 1.01 != 1 (25mins de debug pour ca)
                    {
                        if (!hoveredTeams.Contains(series.Name))
                            hoveredTeams.Add(series.Name);
                    }
                }
            }

            if (hoveredTeams.Count > 0)
            {
                string hoverText = $"Year: {matchedX:F0}\nValue: {matchedY:F2}\nTeams:\n{string.Join("\n", hoveredTeams)}";
                label1.Text = hoverText;
                //met curseur main par ce que c'est plus beau
                formsPlot1.Cursor = Cursors.Hand;
            }
            else
            {
                label1.Text = "";
                formsPlot1.Cursor = Cursors.Default;
            }
        }



        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}