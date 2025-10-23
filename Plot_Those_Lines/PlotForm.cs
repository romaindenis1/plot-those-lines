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
    public partial class PlotForm : Form
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

    // liste des scatters courants (une par serie)
    private List<ScottPlot.Plottables.Scatter> courbesGlobal = new List<ScottPlot.Plottables.Scatter>();
    // quand on change les cases par code, desactive les handlers
    private bool suppressCheckboxEvents = false;

    // palette de couleurs pour les series
        private readonly List<string> palette = new List<string> {
            "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF",
            "#800000", "#008000", "#000080", "#808000", "#800080", "#008080",
            "#FFA500", "#A52A2A", "#5F9EA0", "#D2691E", "#FF7F50", "#6495ED",
            "#DC143C", "#00CED1", "#9400D3", "#FF1493", "#00BFFF", "#228B22",
            "#8B4513", "#2E8B57", "#FF4500", "#DA70D6", "#7FFF00", "#4169E1"
        };

    // map nom serie -> couleur pour la liste
    private readonly Dictionary<string, System.Drawing.Color> seriesColors = new Dictionary<string, System.Drawing.Color>(StringComparer.OrdinalIgnoreCase);

        public PlotForm()
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

                        // met les donnees dans le dictionnaire
                        data.Keys.ToList().
                            ForEach(pos => data[pos].Add(double.TryParse(csv.GetField(pos), out var val)
                            ? val : double.NaN));
                    }

                    // mettre la liste en tableau
                    double[] dataX = years.ToArray();

                    // efface le graphe precedent
                    pltMain.Plot.Clear();
                    allSeriesData.Clear();

                    // stocke les series dans allSeriesData pour hover et toggle
                    data.Select((key, idx) => new { Key = key.Key, Values = key.Value.ToArray(), Index = idx })
                        .ToList()
                        .ForEach(entry =>
                        {
                            allSeriesData.Add(new SeriesData
                            {
                                Name = entry.Key,
                                XValues = dataX,
                                YValues = entry.Values
                            });
                        });

                    // creer checkboxes dynamiques et courbes scottplot
                    if (flpSeries != null)
                    {
                        flpSeries.Controls.Clear();
                    }

                    // vide les courbes existantes
                    pltMain.Plot.Clear();
                    courbesGlobal.Clear();

                    for (int i = 0; i < allSeriesData.Count; i++)
                    {
                        var s = allSeriesData[i];
                        var hex = palette[i % palette.Count];
                        var color = ParseHexColor(hex);
                        seriesColors[s.Name] = color;

                        // ajoute la courbe au graphe
                        var scatter = pltMain.Plot.Add.Scatter(s.XValues, s.YValues);
                        scatter.Color = ScottPlot.Color.FromColor(System.Drawing.Color.FromArgb(color.R, color.G, color.B));
                        scatter.LineWidth = 2;
                        scatter.MarkerSize = 0;
                        scatter.LegendText = s.Name;
                        courbesGlobal.Add(scatter);

                        // creer la checkbox pour cette serie
                        if (flpSeries != null)
                        {
                            var cb = new CheckBox();
                            cb.AutoSize = true;
                            cb.Text = s.Name;
                            cb.Checked = true;
                            cb.ForeColor = color;
                            cb.Margin = new Padding(3, 3, 3, 3);
                            int idx = i; // capture
                            cb.CheckedChanged += (sender, e) =>
                            {
                                if (suppressCheckboxEvents) return;
                                if (idx >= 0 && idx < courbesGlobal.Count)
                                {
                                    courbesGlobal[idx].IsVisible = cb.Checked;
                                    pltMain.Refresh();
                                }
                            };
                            flpSeries.Controls.Add(cb);
                        }
                    }

                    // finalise le graphe
                    pltMain.Plot.Title("Plot");
                    pltMain.Refresh();

                    // dessine selon les cases cochees
                    RenderPlots();

                    // todo: recuperer ces labels depuis le csv
                    pltMain.Plot.XLabel("Year");
                    pltMain.Plot.YLabel("Wins");

                    pltMain.Plot.Legend.IsVisible = false;

                    pltMain.Refresh();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement du CSV : " + ex.Message);
            }
        }


            private void PlotForm_Load(object sender, EventArgs e)
            {
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

        // Render all series to the plot if chkShowData is checked
        private void RenderPlots()
        {
            // use the existing scatters in courbesGlobal and the checkboxes in flpSeries
            if (chkShowData == null || !chkShowData.Checked)
            {
                // hide all
                foreach (var c in courbesGlobal) c.IsVisible = false;
                pltMain.Refresh();
                return;
            }

            // for each checkbox in flpSeries, toggle the corresponding scatter visibility
            for (int i = 0; i < courbesGlobal.Count; i++)
            {
                var scatter = courbesGlobal[i];
                bool visible = true;
                if (flpSeries != null && i < flpSeries.Controls.Count)
                {
                    if (flpSeries.Controls[i] is CheckBox cb)
                        visible = cb.Checked;
                }
                scatter.IsVisible = visible;
            }

            pltMain.Refresh();
        }

        private void chkShowData_CheckedChanged(object sender, EventArgs e)
        {
            // When the global toggle is changed, check/uncheck all series checkboxes
            if (flpSeries != null)
            {
                suppressCheckboxEvents = true;
                bool newState = chkShowData.Checked;
                foreach (Control c in flpSeries.Controls)
                {
                    if (c is CheckBox cb)
                        cb.Checked = newState;
                }
                suppressCheckboxEvents = false;
            }

            // Now update visibility in one go
            RenderPlots();
        }

        // parse #RRGGBB or RRGGBB into a Color; fallback to Black on error
        private System.Drawing.Color ParseHexColor(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return System.Drawing.Color.Black;
            hex = hex.Trim();
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length != 6) return System.Drawing.Color.Black;
            try
            {
                int r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                int g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                int b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                return System.Drawing.Color.FromArgb(r, g, b);
            }
            catch
            {
                return System.Drawing.Color.Black;
            }
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
                    var pointPixel = pltMain.Plot.GetPixel(new ScottPlot.Coordinates(item.x, item.y));
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
