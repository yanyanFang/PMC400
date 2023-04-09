using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PMC400_TEST
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //使用foreach
            for (int i = 1; i < dataGridView1.Rows.Count; i++)
            {
                //获取第i行，列名是列名A的单元格的值
                if (i == 1)
                {
                    double r1 = double.Parse(dataGridView1.Rows[i].Cells["bj"].Value.ToString());//曲率半径

                    double d1 = double.Parse(dataGridView1.Rows[i].Cells["jg"].Value.ToString());//间隔

                    double n1 = double.Parse(dataGridView1.Rows[i].Cells["zsl"].Value.ToString());//折射率

                    double n2 = 0.00;// double.Parse(dataGridView1.Rows[i++].C0.00ells["zsl"].Value.ToString());//折射率

                    double d2 = 0.00;

                    double l = r1 + d2;
                    String expression = SphericalImage(i, r1, n2, n1, l, d1);
                    dataGridView1.Rows[i].Cells["qxx"].Value = expression;
                }
                else {
                
                }
                

            }
        }

        private String SphericalImage(int face, double r, double n2, double n1, double l, double d) {

            String expression = "1+1+2+3";
            object result = new DataTable().Compute(expression, "");
            float.Parse(result + "");
            face--;

            if (face == 1) {
                return "循环结束";
            }
            return expression;
        }
    }
}
