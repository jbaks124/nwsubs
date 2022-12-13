using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Sub
{
    internal class Sub
    {


        class npi
        {
            public string artikel { get; set; }

        }

        class bom
        {

            public string maakartikel { get; set; }
            public string artikel { get; set; }
            public string artsrt { get; set; }

        }

        class routing
        {

            public string maakartikel { get; set; }
            public string routingnr { get; set; }
            public decimal insteltijd { get; set; }
            public decimal stuktijd { get; set; }

        }

        static void Main(string[] args)
        {

            Sub n = new Sub();

            var file_npi = "";
            var file_npi_out = "";
            var file_bom = "";
            var file_routing = "";

            Console.WriteLine("Reading variables..");
            var file2 = Environment.CurrentDirectory + @"\var.txt";
            using (var reader = new StreamReader(file2))
            {

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line.Length > 0)
                    {
                        if (line.Contains("npi_in"))
                        {
                            file_npi = line.Split('\t')[1];
                            Console.WriteLine("Filepath: " + file_npi);
                        }

                        if (line.Contains("dumpuri"))
                        {
                            file_npi_out = line.Split('\t')[1];
                            Console.WriteLine("Worksheet: " + file_npi_out);
                        }

                        if (line.Contains("npibom"))
                        {
                            file_bom = line.Split('\t')[1];
                            Console.WriteLine("Worksheet: " + file_bom);
                        }

                        if (line.Contains("npirouting"))
                        {
                            file_routing = line.Split('\t')[1];
                            Console.WriteLine("Worksheet: " + file_routing);
                        }

                    }
                }
            }

            //var file_npi = Environment.CurrentDirectory + @"\in\npi.txt";
            //var file_npi_out = Environment.CurrentDirectory + @"\out\npi.txt";

            //var file_bom = Environment.CurrentDirectory + @"\in\bom.txt";
            //var file_routing = Environment.CurrentDirectory + @"\in\routing.txt";

            List<npi> npi = new List<npi>();
            List<bom> bom = new List<bom>();
            List<routing> routing = new List<routing>();

            //___Result dict
            IDictionary<string, int> npisub = new Dictionary<string, int>();
            IDictionary<string, int> npisub_count = new Dictionary<string, int>();

            IDictionary<string, decimal> npivk = new Dictionary<string, decimal>();
            IDictionary<string, int> npivk_count = new Dictionary<string, int>();

            var do_sub = true;
            var do_vktijd = true;
            var do_writelist = true;

            var headercheck = 0;
            var stringzero = "0";

            //___read npi
            if (do_sub == true)
            {

                using (var reader = new StreamReader(file_npi))
                {

                    while (!reader.EndOfStream)
                    {

                        var line = reader.ReadLine();
                        var lst = line.Split('|').ToList();

                        if (lst.Count() > 3)
                        {
                            if (headercheck == 0)
                            {
                                headercheck = 1;
                            }
                            else
                            {

                                npi.Add(
                                    new npi()
                                    {
                                        artikel = lst[0].Trim()

                                    });
                            }
                        }
                    }
                }

                //___read bom
                headercheck = 0;

                using (var reader = new StreamReader(file_bom))
                {

                    while (!reader.EndOfStream)
                    {

                        var line = reader.ReadLine();
                        var lst = line.Split('|').ToList();

                        if (lst.Count() > 3)
                        {
                            if (headercheck == 0)
                            {
                                headercheck = 1;
                            }
                            else
                            {

                                bom.Add(
                                    new bom()
                                    {
                                        maakartikel = lst[0].Trim(),
                                        artikel = lst[4].Trim(),
                                        artsrt = lst[18].Trim()

                                    });
                            }
                        }
                    }
                }

                //___Sub

                int outercount = 0;
                int innercount = 0;

                foreach (var i in npi)
                {
                    outercount++;
                    int currentsub = 0;

                    //___1st bom select
                    var bompart = from x in bom
                                  where x.maakartikel == i.artikel &&
                                  x.artsrt == "Maakde"
                                  select x;

                    innercount = bompart.Count();

                    //___fill read out dict
                    IDictionary<string, int> bomnumber = new Dictionary<string, int>();
                    IDictionary<string, int> bomnumber_next = new Dictionary<string, int>();

                    //___Initial item found in bom, sub 1
                    foreach (var i2 in bompart)
                    {
                        if (!bomnumber.ContainsKey(i2.artikel))
                        {
                            bomnumber.Add(i2.artikel, 1);
                            currentsub = 1;
                            //innercount = 1;
                        }

                        Console.WriteLine("{0}.{1}.{2}-{3}: {4}", outercount, innercount, currentsub, i.artikel, i2.artikel);


                    }

                    //___Read out
                    while (true)
                    {

                        if (bomnumber.Count() == 0)
                        {
                            Console.WriteLine("{0}.{1}.{2}-{3}:", outercount, innercount, currentsub, i.artikel);
                            break;
                        }

                        foreach (var i3 in bomnumber)
                        {

                            bompart = from x in bom
                                      where x.maakartikel == (string)i3.Key &&
                                      x.artsrt == "Maakde"
                                      select x;

                            innercount = innercount + bompart.Count();

                            foreach (var i4 in bompart)
                            {

                        

                                if (!bomnumber_next.ContainsKey(i4.artikel))
                                {
                                    bomnumber_next.Add(i4.artikel, i3.Value + 1);
                                    Console.WriteLine("{0}.{1}.{2}-{3}: {4}", outercount, innercount, currentsub, i.artikel, i4.artikel);
                                    currentsub = (i3.Value + 1);
                                }
                            }
                        }

                        bomnumber.Clear();

                        foreach (var element in bomnumber_next)
                            bomnumber.Add(element);

                        bomnumber_next.Clear();

                    }

                    //___Sub dict
                    if (!npisub.ContainsKey(i.artikel))
                    {
                        npisub.Add(i.artikel, currentsub);
                    }

                    //___Sub dict count
                    if (!npisub_count.ContainsKey(i.artikel))
                    {
                        npisub_count.Add(i.artikel, innercount);
                    }
                } 
            }



            //___ VK-tijd
            headercheck = 0;

            if (do_vktijd == true)
            {
                using (var reader = new StreamReader(file_routing))
                {

                    while (!reader.EndOfStream)
                    {

                        var line = reader.ReadLine();
                        var lst = line.Split('|').ToList();

                        if (lst.Count() > 3)
                        {

                            if (headercheck == 0)
                            {
                                headercheck = 1;
                            }
                            else
                            {

                                var t1 = lst[9].Trim();
                                var t2 = lst[10].Trim();

                                if (t1 == "")
                                    t1 = stringzero;
                                if (t1.Contains('.'))
                                    t1 = t1.Replace('.', ',');

                                var t1d = decimal.Parse(t1);

                                if (t2 == "")
                                    t2 = stringzero;
                                if (t2.Contains('.'))
                                    t2 = t2.Replace('.', ',');

                                var t2d = decimal.Parse(t2);

                                routing.Add(
                                    new routing()
                                    {
                                        maakartikel = lst[0].Trim(),
                                        routingnr = lst[3].Trim(),
                                        insteltijd = t1d,
                                        stuktijd = t2d

                                    });
                            }
                        }
                    }
                }


                var query2 = (from t in routing
                              group t by new { t.maakartikel }
                                  into grp
                              select new
                              {
                                  grp.Key.maakartikel,
                                  insteltijd = grp.Sum(t => t.insteltijd),
                                  stuktijd = grp.Sum(t => t.stuktijd),
                                  aantal = grp.Count()
                              });


                foreach (var i in query2)
                {

                    if (!npivk.ContainsKey(i.maakartikel))
                    {
                        npivk.Add(i.maakartikel, i.insteltijd + i.stuktijd);
                    }

                    if (!npivk_count.ContainsKey(i.maakartikel))
                    {
                        npivk_count.Add(i.maakartikel, i.aantal);
                    }
                }
            }


            //___write list npi

            using (var writer = new StreamWriter(file_npi_out))
            {

                headercheck = 0;

                if (do_writelist == true)
                {
                    using (var reader = new StreamReader(file_npi))
                    {
                        while (!reader.EndOfStream)
                        {

                            var line = reader.ReadLine();
                            var lst = line.Split('|').ToList();

                            if (lst.Count() > 3)
                            {
                                if (headercheck == 0)
                                {

                                    headercheck = 1;
                                    writer.WriteLine(line + "VK tijd|VK count|Sub niveaus|Sub item count");

                                }
                                else
                                {

                                    var item = lst[0].Trim();
                                    var vk_val = stringzero;
                                    var vk_count_val = stringzero;
                                    var sub_val = stringzero;
                                    var sub_count_val = stringzero;

                                    if (npivk.ContainsKey(item))
                                    {
                                        vk_val = npivk[item].ToString();
                                    }

                                    if (npivk_count.ContainsKey(item))
                                    {
                                        vk_count_val = npivk_count[item].ToString();
                                    }

                                    if (npisub.ContainsKey(item))
                                    {
                                        sub_val = npisub[item].ToString();
                                    }

                                    if (npisub_count.ContainsKey(item))
                                    {
                                        sub_count_val = npisub_count[item].ToString();
                                    }

                                    writer.WriteLine(line + vk_val + "|" + vk_count_val + "|" + sub_val + "|" + sub_count_val + "|");

                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine("done!");
            //Console.ReadLine();

        }
    }
}
