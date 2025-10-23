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
            pltMain.MouseMove += FormsPlot1_MouseMove;
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

                    //Skip(1) par ce que la premiere collone sont les années
                    headers.Skip(1).ToList().ForEach(header => data[header] = new List<double>());

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
                        data.Keys.ToList().
                            ForEach(pos => data[pos].Add(double.TryParse(csv.GetField(pos), out var val)
                            ? val : double.NaN));
                    }

                    //put list onto array
                    double[] dataX = years.ToArray();

                    //efface le graphe precedent
                    pltMain.Plot.Clear();
                    allSeriesData.Clear();

                    //fixed, maintenant couleurs uniques pour toute la data
                    var palette = new List<string> {
                        "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF",
                        "#800000", "#008000", "#000080", "#808000", "#800080", "#008080",
                        "#FFA500", "#A52A2A", "#5F9EA0", "#D2691E", "#FF7F50", "#6495ED",
                        "#DC143C", "#00CED1", "#9400D3", "#FF1493", "#00BFFF", "#228B22",
                        "#8B4513", "#2E8B57", "#FF4500", "#DA70D6", "#7FFF00", "#4169E1"
                    };

                    pltMain.Refresh();

                    data.Select((key, idx) => new { Key = key.Key, Values = key.Value.ToArray(), Index = idx })
                        .ToList()
                        .ForEach(entry =>
                        {
                            var scatter = pltMain.Plot.Add.Scatter(dataX, entry.Values);
                            scatter.Color = ScottPlot.Color.FromHex(palette[entry.Index % palette.Count]);
                            scatter.LegendText = entry.Key;

                            allSeriesData.Add(new SeriesData
                            {
                                Name = entry.Key,
                                XValues = dataX,
                                YValues = entry.Values
                            });
                        });

                    //TODO: make this get data from CSV 
                    pltMain.Plot.XLabel("Year");
                    pltMain.Plot.YLabel("Wins");

                    pltMain.Plot.Legend.IsVisible = true;

                    pltMain.Refresh();
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
            txtTitle.Text = "Enter your title here...";
        }

    private void btnImport_Click(object sender, EventArgs e)
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

        private void pltMain_Load(object sender, EventArgs e)
        {

        }

        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
            string insertedTitle = txtTitle.Text;
            pltMain.Plot.Title(insertedTitle);
            pltMain.Refresh();
        }
        //trouve et affiche le point le plus proche de la souris
        //ultra consomateur de memoire
        private void FormsPlot1_MouseMove(object sender, MouseEventArgs mouse)
        {
            var mouseCoord = pltMain.Plot.GetCoordinates(mouse.X, mouse.Y);

            double minDistance = double.MaxValue;
            double matchedX = double.NaN;
            double matchedY = double.NaN;
            List<string> hoveredTeams = new List<string>();

            //TODO: CONVERT TO LINQ
            //parcours toutes les series
            // Flatten series into (series, x, y) tuples, skip invalid points, compute pixel distance,
            // filter to points within 50 px and take the closest one.
            var nearest = allSeriesData
                .SelectMany(series => series.XValues.Select((x, i) => new { series, x, y = series.YValues[i] }))
                .Where(item => !double.IsNaN(item.x) && !double.IsNaN(item.y)) //skip points invalides --theoriquement pas besoin par ce que c'est check
                .Select(item =>
                {
                    //calcule distance en pixels entre souris et point
                    var pointPixel = pltMain.Plot.GetPixel(new ScottPlot.Coordinates(series.XValues[i], series.YValues[i]));

                    //calcule difference avec pythogore
                    double diffx = pointPixel.X - mouse.X;
                    double diffy = pointPixel.Y - mouse.Y;
                    double distance = Math.Sqrt(diffx * diffx + diffy * diffy);
                    return new { item.series, item.x, item.y, distance };
                })
                .Where(t => t.distance < 50) //si ce point est plus proche et dans les 50 pixels
                .OrderBy(t => t.distance)
                .FirstOrDefault();

            if (nearest != null)
            {
                minDistance = nearest.distance;
                matchedX = nearest.x;
                matchedY = nearest.y;
            }

            //affiche tooltip ou titre normal
            if (double.IsNaN(matchedX) || double.IsNaN(matchedY))
            {
                pltTeams.Text = "";
                pltMain.Cursor = Cursors.Default;
                return;
            }

            
            hoveredTeams.Clear();
            double tolerance = 1e-6; //nesseaire par ce que 1.00001 != 1, donc comme ca c'est accurate a 1 millionth 

            // Find all series that contain a point matching matchedX/matchedY within tolerance
            //prend valeur absolue avec tolerance 
            // 1.01 != 1 (25mins de debug pour ca)
            var teams = allSeriesData
                .Where(series => series.XValues
                    .Select((x, i) => new { x, y = series.YValues[i] })
                    .Any(p => !double.IsNaN(p.x) && !double.IsNaN(p.y) &&
                              Math.Abs(p.x - matchedX) < tolerance &&
                              Math.Abs(p.y - matchedY) < tolerance))
                .Select(s => s.Name)
                .Distinct()
                .ToList();

            hoveredTeams.AddRange(teams);

            if (hoveredTeams.Count > 0)
            {
                string hoverText = $"Year: {matchedX:F0}\nValue: {matchedY:F2}\nTeams:\n{string.Join("\n", hoveredTeams)}";
                pltTeams.Text = hoverText;
                //met curseur main par ce que c'est plus beau
                pltMain.Cursor = Cursors.Hand;
            }
            else
            {
                pltTeams.Text = "";
                pltMain.Cursor = Cursors.Default;
            }
        }



    private void pltTeams_Click(object sender, EventArgs e)
        {

        }
    }
}