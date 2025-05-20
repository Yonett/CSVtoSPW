using System;
using Kompas6API5;
using KompasAPI7;
using Kompas6Constants;
using Kompas6Constants3D;
using KAPITypes;
using System.Xml.Schema;

namespace CSVtoSPW
{
    public class SpecObject
    {        
        public string Position;
        public string Name;
        public string Count;
        public string Commentary;
        public string Group = String.Empty;
        public string LineBreaks = String.Empty;
        public string GroupName = String.Empty;
        public int[] Numbers;

        public void ParsePosition(ushort LineControlSize)
        {
            int litSize = 0;

            string result = String.Empty;
            string[] words = Position.Split(", ");

            result += words[0];
            litSize += words[0].Length;

            for (int i = 0; i < words.Length - 1; i++)
            {
                if (litSize + words[i + 1].Length > LineControlSize)
                {
                    result += ",\n";
                    LineBreaks += "\n";
                    litSize = 0;
                }
                else
                    result += ", ";

                result += words[i + 1];
                litSize += words[i + 1].Length;
            }
            Position = result;
        }

        public void AnalyzePosition()
        {
            string result = String.Empty;

            int m = 0;
            int k = 0;
            while (m + k < this.Numbers.Length)
            {
                
                while (this.Numbers[m + k] == this.Numbers[m] + k)
                {
                    k++;
                    if ((m + k) >= this.Numbers.Length)
                        break;
                }
                if (1 == k)
                    result += $"{this.Group}{this.Numbers[m]}, ";
                else
                    result += $"{this.Group}{this.Numbers[m]}-{this.Group}{this.Numbers[m + k - 1]}, ";

                m += k;
                k = 0;
            }
            
            if (0 == m)
                result += $"{this.Group}{this.Numbers[0]}";
            else
                result = result[..(result.Length - 2)];

            this.Position = result;
        }

        public SpecObject(string Position, string Name, string Count, string Commentary)
        {
            this.Position = Position;
            this.Name = Name;
            this.Count = Count;
            this.Commentary = Commentary;

            string[] Designations = this.Position.Split(", ");

            int i = 0;

            while (!Char.IsDigit(Designations[0][i]))
            {
                this.Group += Designations[0][i];
                i++;
            }

            this.Numbers = new int[Designations.Length];
            for (int j = 0; j < Designations.Length; j++)
                this.Numbers[j] = Convert.ToInt32(Designations[j][this.Group.Length..]);
        }
    }
    
    internal class Program
    {
        private static Dictionary<string, Tuple<string, string>> Data = new();
        private static List<SpecObject> SpecObjects = new List<SpecObject>();
        private static ushort LineControlSize = 8;

        static void Main(string[] args)
        {
            using (StreamReader Reader = new StreamReader("Data.json"))
            {
                try
                {
                    Data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Tuple<string, string>>>(Reader.ReadToEnd());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            Type? t5 = Type.GetTypeFromProgID("KOMPAS.Application.5", true);
            Type? t7 = Type.GetTypeFromProgID("KOMPAS.Application.7", true);

            if (t5 == null || t7 == null)
                Console.WriteLine("KOMPAS.Application has not been found. Check if KOMPAS is installed");
            else
            {
                Console.WriteLine("KOMPAS.Application has been found. Trying to create instances...");

                Console.WriteLine("Creating KOMPAS.Application.5 instance...");
                KompasObject? kompasApp5 = (KompasObject)Activator.CreateInstance(t5);

                if (kompasApp5 == null)
                    Console.WriteLine("Cannot create KOMPAS.Application.5 instance");
                else
                {
                    Console.WriteLine("KOMPAS.Application.5 instance has been created");

                    Console.WriteLine("Creating KOMPAS.Application.7 instance...");
                    IApplication? kompasApp7 = (IApplication)kompasApp5.ksGetApplication7();

                    if (kompasApp7 == null)
                        Console.WriteLine("Cannot create KOMPAS.Application.7 instance");
                    else
                    {
                        Console.WriteLine("KOMPAS.Application.7 instance has been created");

                        ksSpcDocument iDocumentSpc = (ksSpcDocument)kompasApp5.SpcDocument();

                        ksDocumentParam iDocumentParam  = (ksDocumentParam)kompasApp5.GetParamStruct((short)StructType2DEnum.ko_DocumentParam);

                        iDocumentParam.Init();
                        iDocumentParam.type = (int)DocType.lt_DocSpc;

                        ksSheetPar iSheetParam = (ksSheetPar)iDocumentParam.GetLayoutParam();
                        iSheetParam.Init();
                        iSheetParam.layoutName = @"C:\Program Files\ASCON\KOMPAS-3D V17\Sys\ke.lyt";
                        iSheetParam.shtType = 1;

                        iDocumentSpc.ksCreateDocument(iDocumentParam);

                        IEnumerable<string> lines = File.ReadLines(@"C:\Users\grushev\Desktop\target.csv");

                        foreach (string line in lines)
                        {
                            string[] Words = line.Split(";");
                            SpecObjects.Add(new SpecObject(Words[0], Words[1], Words[2], Words[3]));
                        }

                        ksSpecification iSpc = (ksSpecification)iDocumentSpc.GetSpecification();
                        ksSpcObjParam iSpcObjParam = (ksSpcObjParam)kompasApp5.GetParamStruct((short)StructType2DEnum.ko_SpcObjParam);

                        int i = 0;
                        int reference = 0;

                        string currGroup = "#";
                        string prevGroup = "#";
                        string nextGroup = "#";

                        bool isCurrSingleLine = false;
                        bool isPrevSingleLine = false;
                        bool isNextSingleLine = false;

                        foreach (SpecObject Obj in SpecObjects)
                        {
                            Obj.AnalyzePosition();
                            Obj.ParsePosition(LineControlSize);
                            Obj.GroupName = Data[Obj.Group].Item1;
                        }

                        while (i < SpecObjects.Count)
                        {
                            iSpc.ksSpcObjectCreate("", 0, 1, 0, 0, 2);

                            reference = iSpc.ksSpcObjectEnd();
                            
                            iSpc.ksSpcObjectEdit(reference);

                            iDocumentSpc.ksGetObjParam(reference, iSpcObjParam, ldefin2d.ALLPARAM);
                            iSpcObjParam.blockNumber = 0;
                            iSpcObjParam.draw = 1;
                            iSpcObjParam.firstOnSheet = 0;
                            iSpcObjParam.ispoln = 0;
                            iSpcObjParam.posInc = 1;
                            iSpcObjParam.posNotDraw = 0;
                            iDocumentSpc.ksSetObjParam(reference, iSpcObjParam, ldefin2d.ALLPARAM);
                            
                            currGroup = SpecObjects[i].GroupName;
                            isCurrSingleLine = SpecObjects[i].LineBreaks.Length > 0 ? false : true;

                            #region Getting group names and lines numbers
                            if (i == 0)
                            {
                                prevGroup = "#";
                                nextGroup = SpecObjects[i + 1].GroupName;

                                isPrevSingleLine = false;
                                isNextSingleLine = SpecObjects[i + 1].LineBreaks.Length > 0 ? false : true;
                            }
                            else if (i == SpecObjects.Count - 1)
                            {
                                prevGroup = SpecObjects[i - 1].GroupName;
                                nextGroup = "#";
                                
                                isPrevSingleLine = SpecObjects[i - 1].LineBreaks.Length > 0 ? false : true;
                                isNextSingleLine = false;
                            }
                            else
                            {
                                prevGroup = SpecObjects[i - 1].GroupName;
                                nextGroup = SpecObjects[i + 1].GroupName;

                                isPrevSingleLine = SpecObjects[i - 1].LineBreaks.Length > 0 ? false : true;
                                isNextSingleLine = SpecObjects[i + 1].LineBreaks.Length > 0 ? false : true;
                            }
                            #endregion

                            if (currGroup == prevGroup)
                            {
                                if (isCurrSingleLine)
                                {
                                    if (isPrevSingleLine)
                                    {
                                        iSpc.ksSetSpcObjectColumnText(4, 1, 0, $"{SpecObjects[i].Position}");
                                        iSpc.ksSetSpcObjectColumnText(5, 1, 0, $"{SpecObjects[i].LineBreaks}{SpecObjects[i].Name}");
                                        iSpc.ksSetSpcObjectColumnText(6, 1, 0, $"{SpecObjects[i].LineBreaks}{SpecObjects[i].Count}");
                                        iSpc.ksSetSpcObjectColumnText(7, 1, 0, $"{SpecObjects[i].LineBreaks}{SpecObjects[i].Commentary}");
                                    }
                                    else
                                    {
                                        iSpc.ksSetSpcObjectColumnText(4, 1, 0, $"\n{SpecObjects[i].Position}");
                                        iSpc.ksSetSpcObjectColumnText(5, 1, 0, $"\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Name}");
                                        iSpc.ksSetSpcObjectColumnText(6, 1, 0, $"\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Count}");
                                        iSpc.ksSetSpcObjectColumnText(7, 1, 0, $"\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Commentary}");
                                    }
                                }
                                else
                                {
                                    iSpc.ksSetSpcObjectColumnText(4, 1, 0, $"\n{SpecObjects[i].Position}");
                                    iSpc.ksSetSpcObjectColumnText(5, 1, 0, $"\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Name}");
                                    iSpc.ksSetSpcObjectColumnText(6, 1, 0, $"\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Count}");
                                    iSpc.ksSetSpcObjectColumnText(7, 1, 0, $"\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Commentary}");
                                }
                            }
                            else
                            {
                                if (currGroup == nextGroup)
                                {
                                    if (SpecObjects[i].Group == "A")
                                    {
                                        iSpc.ksSetSpcObjectColumnText(4, 1, 0, $"\n{SpecObjects[i].Position}");
                                        iSpc.ksSetSpcObjectColumnText(5, 1, 0, $"\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Name}");
                                        iSpc.ksSetSpcObjectColumnText(6, 1, 0, $"\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Count}");
                                        iSpc.ksSetSpcObjectColumnText(7, 1, 0, $"\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Commentary}");
                                    }
                                    else
                                    {
                                        iSpc.ksSetSpcObjectColumnText(4, 1, 0, $"\n\n\n{SpecObjects[i].Position}");
                                        iSpc.ksSetSpcObjectColumnText(5, 1, 0, $"\n{Data[SpecObjects[i].Group].Item1}\n\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Name}");
                                        iSpc.ksSetSpcObjectColumnText(6, 1, 0, $"\n\n\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Count}");
                                        iSpc.ksSetSpcObjectColumnText(7, 1, 0, $"\n\n\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Commentary}");
                                    }
                                }
                                else
                                {
                                    iSpc.ksSetSpcObjectColumnText(4, 1, 0, $"\n{SpecObjects[i].Position}");
                                    iSpc.ksSetSpcObjectColumnText(5, 1, 0, $"\n{SpecObjects[i].LineBreaks}{Data[SpecObjects[i].Group].Item2} {SpecObjects[i].Name}");
                                    iSpc.ksSetSpcObjectColumnText(6, 1, 0, $"\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Count}");
                                    iSpc.ksSetSpcObjectColumnText(7, 1, 0, $"\n{SpecObjects[i].LineBreaks}{SpecObjects[i].Commentary}");
                                }
                            }

                            reference = iSpc.ksSpcObjectEnd();

                            i++;
                        }

                        iDocumentSpc.ksSaveDocument(@"C:\Users\grushev\Desktop\target.spw");
                        iDocumentSpc.ksCloseDocument();
                    }

                    kompasApp5.Quit();
                }
            }
        }
    }
}