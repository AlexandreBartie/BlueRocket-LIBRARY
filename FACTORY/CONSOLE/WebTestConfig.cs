﻿using Dooggy.Factory.Data;
using Dooggy.Lib.Data;
using Dooggy.Lib.Files;
using Dooggy;
using System;
using System.Collections.Generic;
using System.Text;
using Dooggy.Lib.Generic;
using Dooggy.Lib.Vars;
using Dooggy.Tools.Calc;
using System.Globalization;

namespace Dooggy.Factory.Console
{
    public class TestConsoleConfig
    {

        public TestConsole Console;

        public TestConfigImport Import;

        public TestConfigMode Mode;

        public TestConfigCSV CSV;
        public TestConfigPath Path;
        public TestConfigTimeout Timeout;

        public TestConfigValidation Validation;

        public TestDataPool Pool => Console.Pool;
        private DataBasesConnection Bases => Pool.Bases;

        public string status() => GetStatus();

        public bool IsOK => Path.IsOK && Bases.IsOK;

        public TestConsoleConfig(TestConsole prmConsole)
        {
            Console = prmConsole;

            Import = new TestConfigImport(this);

            Mode = new TestConfigMode(this);

            CSV = new TestConfigCSV(this);

            Path = new TestConfigPath(this);

            Timeout = new TestConfigTimeout(this);

            Validation = new TestConfigValidation(this);

        }

        public bool Setup(string prmArquivoCFG, bool prmPlay)
        {
            Mode.SetMode(prmPlay);

            if (Import.Setup(prmArquivoCFG))
                { Console.Load(prmPlay); return true; }

            return false;
        }

        public bool Run(string prmBloco) => Import.Run(prmBloco);

        private string GetStatus()
        {
            
            string status = Console.Dados.log;

            if (Console.IsDbOK)
                status += " | " + Timeout.log + " | " + CSV.log + " | " + Path.log;

            status += " | " + Validation.log;

            return status;
      
        }

    }
    public class TestConfigTimeout
    {

        private TestConsoleConfig Config;

        public int connect_timeout = 30;
        public int command_timeout = 20;

        public TestConfigTimeout(TestConsoleConfig prmConfig)
        {
            Config = prmConfig;
        }

        public void SetConnectTimeOut(int prmSegundos) => connect_timeout = prmSegundos;
        public void SetCommandTimeOut(int prmSegundos) => command_timeout = prmSegundos;

        private string txtconnectTimeout() => String.Format("-connectDB: {0}", connect_timeout);
        private string txtcommandTimeout() => String.Format("-commandSQL: {0}", command_timeout);
        public string log => String.Format(">timeout: {0}, {1}", txtconnectTimeout(), txtcommandTimeout());

    }

    public class TestConfigCSV
    {
        private TestConsoleConfig Config;

        public DateTime anchor;

        public string formatRegion;

        public string formatToday;

        public string formatDateDefault;

        private string formatSaveDefault;

        public CultureInfo Culture;

        public TestConfigCSV(TestConsoleConfig prmConfig)
        {
            Config = prmConfig;

            anchor = DateTime.Now;

            formatDateDefault = "DD/MM/AAAA";

            GetCulture();
        }
        public void SetToday(string prmFormat)
        {
            formatToday = prmFormat;

            anchor = myDate.Calc(prmDate: anchor, prmSintaxe: prmFormat);
        }
        public void SetToday(DateTime prmDate)
        {
            anchor = prmDate;
        }
        public void SetFormatDate(string prmFormat)
        {
            formatDateDefault = prmFormat;
        }

        public void SetRegion() => SetRegion(prmRegion: "");
        public void SetRegion(string prmRegion)
        {
            formatRegion = prmRegion; GetCulture();
        }
        public void SetFormatSave(string prmFormat)
        {
            formatSaveDefault = prmFormat;
        }
        
        public string GetSaveDefault()
        {
            string format = "";

            if (myString.IsFull(formatSaveDefault))
                format = string.Format("[{0}]", formatSaveDefault);

            return string.Format(">save{0}:", format);
        }

        public string TextToCSV(string prmText, string prmFormat) => myCSV.TextToCSV(prmText, prmFormat);
        public string DateToCSV(DateTime prmDate, string prmFormat) => myCSV.DateToCSV(prmDate, prmFormat);
        public string DoubleToCSV(Double prmNumber, string prmFormat) => myCSV.DoubleToCSV(prmNumber, prmFormat, prmCulture: Culture);

        private string txt_region => String.Format("-region: {0}", formatRegion);
        private string txt_today => String.Format("-today: {0}", formatToday);
        private string txt_date => String.Format("-date: {0}", formatDateDefault);
        private string txt_save => String.Format("-save: {0}", formatSaveDefault);

        public string log => String.Format(">csv: {0}, {1}, {2}, {3}", txt_region, txt_today, txt_date, txt_save);

        private void GetCulture()
        {
            CultureInfo ret = CultureInfo.InvariantCulture;
            try
            { ret = new CultureInfo(formatRegion); }

            catch (Exception e)
            { }

            Culture = ret;
        }
    }

    public class TestConfigValidation
    {

        private TestConsoleConfig Config;

        private TestConfigCSV CSV => Config.CSV;

        public TestConfigValidation(TestConsoleConfig prmConfig)
        {
            Config = prmConfig;
        }

        public string log => GetTest(prmDate: CSV.anchor, prmNumber: 1234.5);

        private string txt_date(DateTime prmDate) => String.Format("-date: {0}", myCSV.DateToCSV(prmDate, CSV.formatDateDefault));
        private string txt_double(Double prmDouble) => String.Format("-double: {0}", myCSV.DoubleToCSV(prmDouble, CSV.Culture));

        private string GetTest(DateTime prmDate, Double prmNumber) => String.Format(">test: {0}, {1}", txt_date(prmDate), txt_double(prmNumber));

    }
    public class TestConfigMode
    {

        private TestConsoleConfig Config;

        public bool IsAutoPlay;

        public TestConfigMode(TestConsoleConfig prmConfig)
        {
            Config = prmConfig;
        }

        public void SetMode(bool prmAutoPlay) => IsAutoPlay = prmAutoPlay;

    }
    public class TestConfigPath
    {

        private TestConsoleConfig Config;

        private TestTrace Trace => Config.Console.Trace;

        public Diretorio INI;
        public Diretorio OUT;
        public Diretorio LOG;

        public bool IsOK => INI.IsFull && OUT.IsFull && LOG.IsFull;

        public TestConfigPath(TestConsoleConfig prmConfig)
        {
            Config = prmConfig;

            INI = new Diretorio();
            OUT = new Diretorio();
            LOG = new Diretorio();

        }

        public void SetINI(string prmPath)
        {

            INI.SetPath(prmPath);

            Trace.LogPath.SetPath(prmContexto: "OrigemMassaTestes", prmPath);

        }

        public void SetOUT(string prmPath)
        {

            OUT.SetPath(prmPath);

            Trace.LogPath.SetPath(prmContexto: "DestinoMassaTestes", prmPath);

        }
        public void SetLOG(string prmPath)
        {

            LOG.SetPath(prmPath);

            Trace.LogPath.SetPath(prmContexto: "LogMassaTestes", prmPath);

        }

        public string GetExtensao(eTipoFileFormat prmTipo)
        {
            switch (prmTipo)
            {
                case eTipoFileFormat.txt:
                    return "txt";

                case eTipoFileFormat.json:
                    return "json";
            }
            return "csv";
        }
        public eTipoFileFormat GetTipoFormato(string prmTipo)
        {
            switch (prmTipo)
            {
                case "txt":
                    return eTipoFileFormat.txt;

                case "json":
                    return eTipoFileFormat.json;
            }
            return eTipoFileFormat.csv;
        }
        public string GetPathINI() => (INI.path);
        public string GetPathOUT() => (OUT.path);
        public string GetPathOUT(string prmSubPath) => (OUT.GetPath(prmSubPath));
        public string GetPathLOG() => (LOG.path);

        public string GetPathFullOUT(eTipoFileFormat prmTipo) => (GetPathOUT(prmSubPath: GetExtensao(prmTipo)));

        public string log => String.Format(">path: -ini: '{0}', -out: '{1}', -log: '{2}'", INI.path, OUT.path, LOG.path);

    }
    public class TestConfigImport
    {
        public TestConsoleConfig Config;

        private TestConsole Console => Config.Console;

        private TestTrace Trace => Console.Trace;
        private TestDataConnect Connect => Console.Pool.Connect;

        private FileTXT File;

        public bool IsOK;

        private string pathCFG;
        private string nome_completo => File.nome_completo;

        private string grupo;

        private string linha;

        private string prefixo_grupo = ">";
        private string prefixo_parametro = "-";

        private string delimitador = ":";

        private string var_pathCFG = "#(PathArquivoCFG)";

        public TestConfigImport(TestConsoleConfig prmConfig)
        {
            Config = prmConfig;
        }

        public bool Setup(string prmArquivoCFG)
        {

            File = new FileTXT(); IsOK = false;

            if (File.Open(prmArquivoCFG))
            {

                pathCFG = File.path;

                if (Run(prmBloco: File.txt()))
                { Trace.LogConfig.LoadConfig(prmArquivoCFG: nome_completo, prmPathCFG: pathCFG); IsOK = true; return true; }
                else
                { Trace.LogConfig.FailLoadConfig(Config.status(), prmArquivoCFG: nome_completo, prmPathCFG: pathCFG); return (false); }

            }

            Trace.LogFile.FailDataFileOpen(nome_completo, File.path);

            return false;
        }
        public bool Run(string prmBloco)
        {
            foreach (string line in new xMemo(prmBloco, prmSeparador: Environment.NewLine))
            {
                if (SetLine(line))
                {
                    if (IsGroup())
                        SetGroup();
                    else
                        SetParameter();
                }
            }

            return Config.Path.IsOK;
        }

        public bool SetLine(string prmLinha)
        {

            if (myString.IsFull(prmLinha))
                { linha = prmLinha; return true; }

            return false;

        }

        public void SetParameter()
        {

            string tag; string valor; string sigla;
      
            tag = Bloco.GetBloco(linha, prmDelimitadorInicial: prefixo_parametro, prmDelimitadorFinal: delimitador).Trim().ToLower();
            valor = Bloco.GetBlocoDepois(linha, delimitador, prmTRIM: true);

            sigla = Bloco.GetBloco(tag, prmDelimitadorInicial: "[", prmDelimitadorFinal: "]").Trim().ToLower();

            if (sigla != "")
                tag = Bloco.GetBlocoAntes(tag, prmDelimitador: "[",prmTRIM: true).ToLower();

            switch (grupo)
            {

                case "path":
                    SetGroupPath(tag, valor); break;

                case "dbase":
                    SetGroupDBase(tag, sigla, valor); break;

                case "timeout":
                    SetGroupTimeout(tag, valor); break;

                case "csv":
                    SetGroupCSV(tag, valor); break;

                default:
                    Trace.LogConfig.FailFindGroup(grupo); break;
            }

        }

        private void SetGroupPath(string prmTag, string prmValor)
        {

            string path = myString.GetSubstituir(prmValor, var_pathCFG, pathCFG);
            
            switch (prmTag)
            {
                case "ini":
                    Config.Path.SetINI(path); break;

                case "out":
                    Config.Path.SetOUT(path); break;

                case "log":
                    Config.Path.SetLOG(path); break;

                default:
                    Trace.LogConfig.FailFindParameter(prmTag, path); break;
            }
        }
        private void SetGroupDBase(string prmTag, string prmSigla, string prmValor)
        {
            switch (prmTag)
            {
                case "db":
                    Connect.Oracle.AddJSON(prmSigla, prmValor); break;

                default:
                    Trace.LogConfig.FailFindParameter(prmSigla, prmValor); break;
            }
        }
        private void SetGroupTimeout(string prmTag, string prmValor)
        {
            switch (prmTag)
            {
                case "connectdb":
                    Config.Timeout.SetConnectTimeOut(myInt.GetNumero(prmValor)); break;

                case "commandsql":
                    Config.Timeout.SetCommandTimeOut(myInt.GetNumero(prmValor)); break;

                default:
                    Trace.LogConfig.FailFindParameter(prmTag, prmValor); break;
            }
        }
        private void SetGroupCSV(string prmTag, string prmValor)
        {
            switch (prmTag)
            {
                case "region":
                    Config.CSV.SetRegion(prmValor); break;

                case "today":
                    Config.CSV.SetToday(prmValor); break;

                case "date":
                    Config.CSV.SetFormatDate(prmValor); break;

                case "save":
                    Config.CSV.SetFormatSave(prmValor); break;

                default:
                    Trace.LogConfig.FailFindParameter(prmTag, prmValor); break;
            }
        }
        private bool IsGroup() => (Prefixo.IsPrefixo(linha, prefixo_grupo, delimitador));
        private void SetGroup() => grupo = (Prefixo.GetPrefixo(linha, prefixo_grupo, delimitador).Trim().ToLower());

    }

}




