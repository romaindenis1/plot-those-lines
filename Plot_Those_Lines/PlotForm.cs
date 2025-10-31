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
        //modele memoire pour series
        private List<SeriesData> allSeriesData = new List<SeriesData>();

        //plottables scatter traces
        private List<ScottPlot.Plottables.Scatter> courbesGlobal = new List<ScottPlot.Plottables.Scatter>();

        //map nomserie -> couleur
        private Dictionary<string, System.Drawing.Color> seriesColors = new Dictionary<string, System.Drawing.Color>(StringComparer.OrdinalIgnoreCase);

        //palette hex sans #
        private List<string> palette = new List<string> { "1f77b4", "ff7f0e", "2ca02c", "d62728", "9467bd", "8c564b", "e377c2", "7f7f7f", "bcbd22", "17becf" };

        //etat ui controle
        private bool suppressCheckboxEvents = false;

        //chemin csv defaut utilise par l application
        private string csvFilePath = System.IO.Path.Combine(Application.StartupPath, "data.csv");

        //conteneur simple pour series
        private class SeriesData
        {
            public string Name { get; set; }
            public double[] XValues { get; set; }
            public double[] YValues { get; set; }
        }

        public PlotForm()
        {
            InitializeComponent();
            // Ensure mouse move events are handled even if the designer wiring was removed.
            // This avoids losing hover functionality when Designer.cs is modified.
            try
            {
                this.pltMain.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FormsPlot1_MouseMove);
            }
            catch
            {
                // ignore if pltMain isn't initialized yet during designer operations
            }
        }

        private void LoadCsvAndPlot(string path)
        {
            try
            {
                //lire csv en annees plus dictionnaire de donnees
                var (newYears, newData) = ReadCsvData(path);

                if (newYears.Length == 0)
                    return;

                //si on a deja des donnees decider fusion ou remplacement selon schema annees
                if (allSeriesData != null && allSeriesData.Count > 0)
                {
                    //recuperer noms series et annees existantes via linq
                    var existingNames = allSeriesData.Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var newNames = newData.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var existingYears = allSeriesData.SelectMany(s => s.XValues).Distinct().ToArray();

                    bool sameSchema = existingNames.SetEquals(newNames);
                    bool yearsOverlap = existingYears.Intersect(newYears).Any();

                    if (!sameSchema || yearsOverlap)
                    {
                        //donnees entrantes en conflit remplacer precedent dataset silencieusement
                        ResetData();
                        var dataX = newYears;
                        allSeriesData = newData.Select(kv => new SeriesData { Name = kv.Key, XValues = dataX, YValues = kv.Value }).ToList();
                    }
                    else
                    {
                        //meme schema et pas de recoupement -> fusionner
                        MergeIntoAllSeriesData(newYears, newData);
                    }
                }
                else
                {
                    //initialiser allseriesdata depuis newdata
                    allSeriesData.Clear();
                    double[] dataX = newYears;
                    allSeriesData = newData.Select(kv => new SeriesData { Name = kv.Key, XValues = dataX, YValues = kv.Value }).ToList();
                }

                //rafraichir graphe et ui depuis allseriesdata
                UpdatePlotAndUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement du CSV : " + ex.Message);
            }
        }

        //lire csv retourner annees tableau dictionnaire nom->yvalues
        private (double[] years, Dictionary<string, double[]> data) ReadCsvData(string path)
        {
            var years = new List<double>();
            var data = new Dictionary<string, List<double>>();

            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                var headers = csv.HeaderRecord;

                //preparer listes pour chaque colonne entete sauf premiere annee
                data = headers.Skip(1).ToDictionary(h => h, h => new List<double>());

                while (csv.Read())
                {
                    if (!double.TryParse(csv.GetField(headers[0]), out var yearVal))
                    {
                        years.Add(double.NaN);
                    }
                    else
                    {
                        years.Add(yearVal);
                    }

                    data.Keys.ToList().ForEach(key =>
                    {
                        var field = csv.GetField(key);
                        if (double.TryParse(field, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                            data[key].Add(v);
                        else
                            data[key].Add(double.NaN);
                    });
                }
            }

            return (years.ToArray(), data.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray()));
        }

        //fusionner csv entrant dans allseriesdata aligner par annee
        private void MergeIntoAllSeriesData(double[] newYears, Dictionary<string, double[]> newData)
        {
            //construire union des annees
            var existingYears = allSeriesData.SelectMany(s => s.XValues).Distinct().ToList();
            var unionYears = existingYears.Union(newYears).Distinct().OrderBy(y => y).ToArray();

            //construire map a partir des entrees existantes et nouvelles en une seule passe
            var existingEntries = allSeriesData
                .SelectMany(s => s.XValues.Select((x, i) => new { y = x, name = s.Name, val = s.YValues[i] }));

            var newEntries = newData
                .SelectMany(kv => kv.Value.Select((v, i) => new { y = (i < newYears.Length ? newYears[i] : double.NaN), name = kv.Key, val = v }));

            var entries = existingEntries.Concat(newEntries)
                .Where(e => !double.IsNaN(e.y));

            var map = entries
                .GroupBy(e => e.y)
                .ToDictionary(g => g.Key, g => g.ToDictionary(a => a.name, a => a.val, StringComparer.OrdinalIgnoreCase));

            //reconstruire allseriesdata pour uniondesannees collecter valeurs par serie nan si manquant
            var seriesNames = map.Values.SelectMany(d => d.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var newSeriesList = seriesNames
                .Select(name => new SeriesData
                {
                    Name = name,
                    XValues = unionYears,
                    YValues = unionYears.Select(year => map.TryGetValue(year, out var dict) && dict.TryGetValue(name, out var v) ? v : double.NaN).ToArray()
                })
                .ToList();

            allSeriesData = newSeriesList;
        }

        //reconstruire graphe et controles ui depuis allseriesdata
        private void UpdatePlotAndUI()
        {
            if (flpSeries != null) flpSeries.Controls.Clear();
            pltMain.Plot.Clear();
            courbesGlobal.Clear();

            //construit graphique
            allSeriesData
                .Select((s, i) => new { s, i })
                .ToList()
                .ForEach(item =>
                //settings
                {
                    var s = item.s;
                    var i = item.i;
                    var hex = palette[i % palette.Count];
                    var color = ParseHexColor(hex);
                    seriesColors[s.Name] = color;
                    var scatter = pltMain.Plot.Add.Scatter(s.XValues, s.YValues);
                    scatter.Color = ScottPlot.Color.FromColor(System.Drawing.Color.FromArgb(color.R, color.G, color.B));
                    scatter.LineWidth = 2;
                    scatter.MarkerSize = 0;
                    scatter.LegendText = s.Name;
                    courbesGlobal.Add(scatter);

                    if (flpSeries != null)
                    {
                        //ajoute courbe au graphe et creer checkbox
                        var cb = new CheckBox();
                        cb.AutoSize = true;
                        cb.Text = s.Name;
                        cb.Checked = true;
                        cb.ForeColor = color;
                        cb.Margin = new Padding(3, 3, 3, 3);
                        int idx = i;
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
                });

            pltMain.Plot.XLabel("Year");                //x
            pltMain.Plot.YLabel("Wins");                //y
            pltMain.Plot.Legend.IsVisible = false;      //j'utilise pas la legende, mais elle est active de base
            pltMain.Refresh();
        }

        private void PlotForm_Load(object sender, EventArgs e)
        {
            //initialiser titre par defaut
            txtTitle.Text = "Enter your title here...";

            //si un data.csv existe au demarrage charger et afficher
            try
            {
                if (File.Exists(csvFilePath))
                    LoadCsvAndPlot(csvFilePath);
            }
            catch
            {
            }
        }

        //logique pour l'import dun fichier
        private void btnImport_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = @"C:\";           //defini le drive c par default
                openFileDialog.Filter = "CSV files (*.csv)|*.csv";  //dit de prendre que les fichiers CSV
                openFileDialog.FilterIndex = 1;                     //dit de prendre la premiere possibility avant (index commence a 1)

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFile = openFileDialog.FileName;

                    //si les fichiers sont jugees identiques par filecompare
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
        //compare fichier importe avec fichier que le user veux importer
        private bool FileCompare(string file1, string file2)
        {

            if (file1 == file2)
                return true;

            using (FileStream fs1 = new FileStream(file1, FileMode.Open))
            using (FileStream fs2 = new FileStream(file2, FileMode.Open))
            {
                //si pas meme longueur -- pas le meme fichier
                if (fs1.Length != fs2.Length)
                    return false;

                int file1byte;
                int file2byte;

                //lit octets jusqu a fin ou mismatch
                do
                {
                    file1byte = fs1.ReadByte();
                    file2byte = fs2.ReadByte();
                }
                while ((file1byte == file2byte) && (file1byte != -1));

                //retourne bool
                return (file1byte == file2byte);
            }
        }

        private void pltMain_Load(object sender, EventArgs e)
        {

        }

        //change le titre scottplot a partir du titre de la textbox
        private void txtTitle_TextChanged(object sender, EventArgs e)
        {
            string insertedTitle = txtTitle.Text;
            pltMain.Plot.Title(insertedTitle);
            pltMain.Refresh();
        }

        //render toutes series vers graphe si chkshowdata coche
        private void RenderPlots()
        {
            //utiliser scatters existants dans courbesglobal et checkboxes dans flpseries
            if (chkShowData == null || !chkShowData.Checked)
            {
                //cacher tout
                courbesGlobal.ForEach(c => c.IsVisible = false);
                pltMain.Refresh();
                return;
            }

            //pour chaque checkbox dans flpseries basculer visibilite scatter correspondant
            Enumerable.Range(0, courbesGlobal.Count).ToList().ForEach(i =>
            {
                var scatter = courbesGlobal[i];
                bool visible = true;
                if (flpSeries != null && i < flpSeries.Controls.Count)
                {
                    if (flpSeries.Controls[i] is CheckBox cb)
                        visible = cb.Checked;
                }
                scatter.IsVisible = visible;
            });

            pltMain.Refresh();
        }

        private void chkShowData_CheckedChanged(object sender, EventArgs e)
        {
            //quand toggle global change cocher/decocher toutes les checkboxes series
            if (flpSeries != null)
            {
                suppressCheckboxEvents = true;
                bool newState = chkShowData.Checked;
                flpSeries.Controls.OfType<CheckBox>().ToList().ForEach(cb => cb.Checked = newState);
                suppressCheckboxEvents = false;
            }

            //mettre a jour visibilite en une fois
            RenderPlots();
        }

        //parse hex rrggbb en couleur fallback noir en cas d erreur
        private System.Drawing.Color ParseHexColor(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) return System.Drawing.Color.Black;
            hex = hex.Trim();
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length != 6) return System.Drawing.Color.Black;
            //return rbg
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

        //clear toutes les donnees chargees graphe et controles ui pour remplacer proprement
        private void ResetData()
        {
            try
            {
                //reinitialiser modele memoire des series
                if (allSeriesData == null)
                    allSeriesData = new List<SeriesData>();
                else
                    allSeriesData.Clear();

                //effacer series tracees
                if (courbesGlobal != null)
                    courbesGlobal.Clear();

                //effacer couleurs stockees
                if (seriesColors != null)
                    seriesColors.Clear();

                //effacer liste ui des series
                if (flpSeries != null)
                    flpSeries.Controls.Clear();

                //effacer le graphe
                if (pltMain != null)
                {
                    pltMain.Plot.Clear();
                    pltMain.Refresh();
                }

                //assurer etat coherant checkbox globale
                if (chkShowData != null)
                {
                    suppressCheckboxEvents = true;
                    chkShowData.Checked = true;
                    suppressCheckboxEvents = false;
                }
            }
            catch
            {
                //ne pas planter ui si reset echoue
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

            //map series dans des tuples (series, x, y), en sautant points non valide, calcule distance du pixel
            //filtre sur les points a moins de 50px et prend le plus proche
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


            //trouve toutes series qui corespondent avec matchedy and matchedx prend valeur absolue avec tolerance 
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

            //si il y a une equipe survole
            if (hoveredTeams.Count > 0)
            {
                string hoverText = $"Year: {matchedX:F0}\nValue: {matchedY:F2}\nTeams:\n{string.Join("\n", hoveredTeams)}";
                pltTeams.Text = hoverText;
                //met curseur main par ce que c'est plus beau
                pltMain.Cursor = Cursors.Hand;
            }
            //else reset a default
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
