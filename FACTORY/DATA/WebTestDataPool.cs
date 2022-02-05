﻿using Dooggy;
using Dooggy.Factory;
using Dooggy.Factory.Console;
using Dooggy.Lib.Data;
using Dooggy.Lib.Files;
using Dooggy.Lib.Generic;
using Dooggy.Lib.Parse;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Dooggy.Factory.Data
{

    public enum eTipoFileFormat : int
    {
        padrao = 0,
           txt = 1,
           csv = 2,
          json = 3
    }

    public class TestDataPool
    {

        public TestFactory Factory;

        public DataBasesConnection Bases;

        public TestDataConnect Connect;

        public TestDataLocal Dados;

        public TestDataVars Vars;
        public TestDataRaws Raws;
        public TestDataViews Views;

        public TestDataTratamento Tratamento;

        public TestConsole Console => Factory.Console;
        public TestTrace Trace => Factory.Trace;

        private bool bloqueado = false;

        public TestDataFluxos Fluxos => (DataViewCorrente.Fluxos);

        public DataBaseConnection DataBaseCorrente => (Bases.Corrente);

        public TestDataView DataViewCorrente => (Views.Corrente);
        public TestDataFluxo DataFluxoCorrente => (Fluxos.Corrente);

        public bool IsDbOK => (Bases.IsOK);
        public bool IsDbBlocked => bloqueado;
        public bool IsHaveData => Raws.IsHaveData || Views.IsHaveData;
        private int next_view => Views.Count + 1;

        public TestDataPool(TestFactory prmFactory)
        {

            Factory = prmFactory;

            Bases = new DataBasesConnection();

            Dados = new TestDataLocal(this);

            Connect = new TestDataConnect(this);

            Tratamento = new TestDataTratamento(this);

            Cleanup();

        }

        public bool DoConnect() => Bases.DoConnect();

        public bool AddDataBase(string prmTag, string prmConexao) => Bases.Criar(prmTag, prmConexao, this);

        public string AddDataVar(string prmVar) => Vars.Criar(prmVar, DataBaseCorrente);

        public string AddDataView(string prmTag) => AddDataView(prmTag, prmMask: "");
        public string AddDataView(string prmTag, string prmMask) => Views.Criar(prmTag, prmMask, DataBaseCorrente);

        public bool AddDataFluxo(string prmTag, string prmSQL, string prmMask) => DataViewCorrente.Fluxos.Criar(prmTag, prmSQL, prmMask, DataViewCorrente);

        public void SetDataRaw(string prmOptions) => Raws.SetOptions(prmOptions);
        public void AddDataRaw(string prmArg, string prmInstrucao) => Raws.SetArgumento(prmArg, prmInstrucao);

        public void SetDataVar(string prmArg, string prmInstrucao) => Vars.SetArgumento(prmArg, prmInstrucao);
        public void SetDataView(string prmArg, string prmInstrucao) => Views.SetArgumento(prmArg, prmInstrucao);
        public void SetDataFluxo(string prmArg, string prmInstrucao) => Fluxos.SetArgumento(prmArg, prmInstrucao);
        public void SetMaskDataFluxo(string prmMask) => Fluxos.SetMask(prmMask);

        public void Cleanup()
        {

            Vars = new TestDataVars(this);
            Raws = new TestDataRaws(this);
            Views = new TestDataViews(this);

        }

        public void SetDBStatus(bool prmBloqueado) => bloqueado = prmBloqueado;

        public bool IsSQLDataException(string prmTexto) => Tratamento.IsSQLDataException(prmTexto);
        public string GetTextoTratado(string prmTexto) => Tratamento.GetTextoTratado(prmTexto);
        public string GetNextKeyDataView() => string.Format("view#{0},", next_view);

        public string txt(string prmTags) => output(prmTags, prmTipo: eTipoFileFormat.txt);
        public string csv(string prmTags) => output(prmTags, prmTipo: eTipoFileFormat.csv);
        public string json(string prmTags) => output(prmTags, prmTipo: eTipoFileFormat.json);
        public string output(string prmTags, eTipoFileFormat prmTipo) => Tratamento.GetOutput(prmTags, prmTipo);

    }

    public class TestDataTratamento : TestDataFormat
    {
        private TestDataVars Vars => Pool.Vars;
        private TestDataRaws Raws => (Pool.Raws);
        private TestDataViews Views => (Pool.Views);

        public TestDataTratamento(TestDataPool prmPool)
        {

            Pool = prmPool;

        }
        public string GetTextoTratado(string prmTexto)
        {

            string texto = prmTexto;

            texto = GetSQLVariavel(texto);
            texto = GetSQLFuncoes(texto);
            
            return (texto);

        }
        public string GetOutput(string prmTags, eTipoFileFormat prmTipo)
        {

            string data_view = Views.output(prmTags, prmTipo);

            if (Raws.IsON)
                return Raws.GetOutput(data_view, prmTipo);

            return data_view;

        }
        private string GetVariavel(string prmTag)
        {

            if (Vars.Find(prmTag))
                return Vars.Corrente.valor;

            return ("");

        }
        private string GetFuncao(string prmFuncao, string prmParametro)
        {

            switch (prmFuncao)
            {

                case "date":
                    return GetDataDinamica(prmParametro);

                case "now":
                case "today":
                    return GetDataDinamica(prmParametro);

            }

            return ("");
        }

        private string GetDataDinamica(string prmParametro)
        {

            DynamicDate Date = new DynamicDate(anchor);

            return (Date.View(prmSintaxe: prmParametro));

        }

        private string GetDataEstatica(string prmParametro)
        {

            DynamicDate Date = new DynamicDate(anchor);

            return (Date.Static(prmSintaxe: prmParametro));

        }
        private string GetSQLVariavel(string prmTexto)
        {

            string sql = prmTexto; string var; string var_extendido; string var_valor;

            while (true)
            {

                var_extendido = Bloco.GetBloco(sql, prmDelimitadorInicial: "$(", prmDelimitadorFinal: ")$", prmPreserve: true);

                var = Bloco.GetBloco(sql, prmDelimitadorInicial: "$(", prmDelimitadorFinal: ")$");

                if (var == "") break;

                var_valor = GetVariavel(prmTag: var);

                sql = xString.GetSubstituir(sql, var_extendido, var_valor);

                if (var_valor == "")
                    Trace.LogConsole.FailFindValueVariable(var, prmTexto);

            }

            return (sql);

        }

        private string GetSQLFuncoes(string prmSQL)
        {

            string sql = prmSQL; string funcao; string funcao_ext;
            string prefixo; string parametro; string valor;

            while (true)
            {

                funcao = Bloco.GetBloco(sql, prmDelimitadorInicial: "$", prmDelimitadorFinal: "(");

                funcao_ext = Bloco.GetBloco(sql, prmDelimitadorInicial: "$", prmDelimitadorFinal: "(", prmPreserve: true);

                prefixo = Bloco.GetBloco(sql, prmDelimitadorInicial: funcao_ext, prmDelimitadorFinal: ")$", prmPreserve: true);

                parametro = Bloco.GetBloco(sql, prmDelimitadorInicial: funcao_ext, prmDelimitadorFinal: ")$");

                if ((xString.IsEmpty(funcao)) || (xString.IsEmpty(parametro))) break;

                valor = GetFuncao(funcao, parametro);

                if (valor != "")
                    sql = xString.GetSubstituir(sql, prefixo, valor);

            }

            return (sql);

        }

    }
    public class TestDataFormat : TestDataException
    {

        public TestConsoleConfig Config => Pool.Console.Config;

        public DateTime anchor => Config.Format.anchor;

        public string dateFormatDefault => Config.Format.dateFormatDefault;

        public string GetDateAnchor() => GetDateAnchor(dateFormatDefault);
        public string GetDateAnchor(string prmFormatacao) => GetDateFormat(anchor, prmFormatacao);


        public string GetNumberFormat(double prmNumber) => GetNumberFormat(prmNumber, prmFormatacao: "C2");
        public string GetNumberFormat(double prmNumber, string prmFormatacao) => prmNumber.ToString(prmFormatacao);

        public string GetDateFormat(DateTime prmData) => GetDateFormat(prmData, dateFormatDefault);
        public string GetDateFormat(DateTime prmData, string prmFormatacao) => xDate.GetFormatacao(prmData, prmFormatacao);

    }
    public class TestDataException
    {

        public TestDataPool Pool;

        private xLista Dominio;

        private string dataSQL_ZeroItens { get => GetTag("ZeroItensSQL"); }
        private string dataSQL_NoCommand { get => GetTag("NoCommandSQL"); }

        public bool IsSQLDataException(string prmItem) => (Dominio.GetContido(prmItem) != 0);

        private string GetTag(string prmTexto) => string.Format("<#$#{0}#$#>", prmTexto);

        public TestTrace Trace => (Pool.Trace);

        public TestDataException()
        {

            PopularDominio();

        }

        private void PopularDominio()
        {
            Dominio = new xLista();

            Dominio.Add(dataSQL_ZeroItens);
            Dominio.Add(dataSQL_NoCommand);

        }

        public string GetZeroItens()
        {

            return (dataSQL_ZeroItens);

        }

        public string GetNoCommand()
        {

            Trace.LogData.SQLNoCommand();

            return (dataSQL_NoCommand);

        }
    }

    public class TestDataLocal
    {

        public TestDataPool Pool;

        public TestDataFile FileINI;
        public TestDataFile FileLOG;
        public TestDataView View { get => Pool.DataViewCorrente; }

        public TestTrace Trace { get => Pool.Trace; }

        public bool IsHaveData => Pool.IsHaveData;

        public TestDataLocal(TestDataPool prmPool)
        {

            Pool = prmPool;

            FileINI = new TestDataFile(this, prmExtensao: "ini");
            FileLOG = new TestDataFile(this, prmExtensao: "log");

        }

        public bool DoConnect() => Pool.DoConnect();

        public string AddDataView(string prmTag) => (AddDataView(prmTag, prmMask: ""));
        public string AddDataView(string prmTag, string prmMask) => (Pool.AddDataView(prmTag, prmMask));

        public bool AddDataFluxo(string prmTag) => (AddDataFluxo(prmTag, prmSQL: ""));
        public bool AddDataFluxo(string prmTag, string prmSQL) => (AddDataFluxo(prmTag, prmSQL, prmMask: ""));
        public bool AddDataFluxo(string prmTag, string prmSQL, string prmMask) => (Pool.AddDataFluxo(prmTag, prmSQL, prmMask));

        public string txt(string prmTags) => (Pool.txt(prmTags));
        public string csv(string prmTags) => (Pool.csv(prmTags));
        public string json(string prmTags) => (Pool.json(prmTags));
        public string output(string prmTags, eTipoFileFormat prmTipo) => (Pool.output(prmTags, prmTipo));

        public string log => Pool.Bases.log();

    }

    public class ITestDataLocal
    {

        private TestDataLocal _Dados;

        public TestDataLocal Dados
        {
            get
            {
                if (_Dados == null)
                    _Dados = null; // new TestDataLocal();

                return _Dados;

            }

        }

    }
}
