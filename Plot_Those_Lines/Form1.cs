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
using CsvHelper;
using CsvHelper.Configuration;
namespace Plot_Those_Lines
{
    public partial class Form1 : Form
    {
        public Form1()
        {
                InitializeComponent();
                double[] dataX = years.ToArray();

                //formsPlot1.Plot.Clear();

                foreach (var kvp in data)
                {
                    double[] dataY = kvp.Value.ToArray();
                    var scatter = formsPlot1.Plot.Add.Scatter(dataX, dataY);
                    scatter.Label = kvp.Key;
                }

                formsPlot1.Plot.Title("Player Heights by Position Over Years");
                formsPlot1.Plot.XLabel("Year");
                formsPlot1.Plot.YLabel("Height");
                formsPlot1.Plot.Legend.IsVisible = true;

                formsPlot1.Refresh();

            }
            finally
            {
                reader.Dispose();
            }


        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
