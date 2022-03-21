using System;
using System.Text;
using System.Web.UI;
using BoletoNet;
using BoletoNet.Util;

[assembly: WebResource("BoletoNet.Imagens.422.jpg", "image/jpg")]

namespace BoletoNet
{
    /// <author>  
    /// Eduardo Frare
    /// Stiven 
    /// Diogo
    /// Miamoto
    /// </author>    

    ///<summary>
    /// Classe referente ao Banco Banco_Safra
    ///</summary>
    internal class Banco_Safra : AbstractBanco, IBanco
    {
        private string _dacNossoNumero = string.Empty;
        private int _dacContaCorrente = 0;
        private int _dacBoleto = 0;

        /// <summary>
        /// Classe responsavel em criar os campos do Banco Banco_Safra.
        /// </summary>
        internal Banco_Safra()
        {
            this.Codigo = (int)Enums.Bancos.Safra;
            this.Digito = "7";
            this.Nome = "Banco_Safra";
        }

        /// <summary>
        /// Calcula o digito do Nosso Numero
        /// </summary>
        public string CalcularDigitoNossoNumero(Boleto boleto)
        {
            if (boleto.NossoNumero.Length < 9)
            {
                throw new IndexOutOfRangeException("Erro. O campo 'Nosso Número' deve ter mais de 9 digitos. Você digitou " + boleto.NossoNumero);
            }

            string sfNossoNumero = boleto.NossoNumero.Substring(0, 8);
            int sfDigitoNossoNumero = Mod11(sfNossoNumero, 9, 0);
            string sfDigito = "";

            if (sfDigitoNossoNumero == 0)
                sfDigito = "1";
            else if (sfDigitoNossoNumero > 1)
                sfDigito = Convert.ToString(11 - sfDigitoNossoNumero);

            return sfDigito;
        }


        /// <summary>       
        /// SISTEMA	        020	020	7	FIXO
        /// CLIENTE	        021	034	CÓDIGO DO CLIENTE	CÓDIGO/AGÊNCIA CEDENTE
        /// N/NÚMERO	    035	043	NOSSO NÚMERO	NOSSO NÚMERO DO TÍTULO
        /// TIPO COBRANÇA	044	044	2	FIXO
        /// </summary>
        public string CampoLivre(Boleto boleto)
        {

            string campolivre = "7" + boleto.Cedente.ContaBancaria.Conta.ToString() + boleto.Cedente.ContaBancaria.Agencia.ToString() +
                                boleto.NossoNumero.Substring(0, 9) + "2";
            return campolivre;
        }

        #region IBanco Members
        public override string GerarHeaderRemessa(string numeroConvenio, Cedente cedente, TipoArquivo tipoArquivo, int numeroArquivoRemessa, Boleto boletos)
        {
            throw new NotImplementedException("Função não implementada.");
        }

        public override string GerarHeaderRemessa(Cedente cedente, TipoArquivo tipoArquivo, int numeroArquivoRemessa)
        {
            return GerarHeaderRemessa("0", cedente, tipoArquivo, numeroArquivoRemessa);
        }

        public override string GerarHeaderRemessa(string numeroConvenio, Cedente cedente, TipoArquivo tipoArquivo, int numeroArquivoRemessa)
        {
            try
            {
                string header = " ";

                base.GerarHeaderRemessa("0", cedente, tipoArquivo, numeroArquivoRemessa);

                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        header = GerarHeaderRemessaCNAB240(cedente, numeroArquivoRemessa);
                        break;
                    case TipoArquivo.CNAB400:
                        throw new NotImplementedException("Remessa não implementada!");
                    case TipoArquivo.Outro:
                        throw new Exception("Tipo de arquivo inexistente.");
                }

                return header;

            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a geração do HEADER do arquivo de REMESSA.", ex);
            }
        }

        private string GerarHeaderRemessaCNAB240(Cedente cedente, int numeroArquivoRemessa)
        {
            try
            {
                StringBuilder header = new StringBuilder(240);

                header.Append(Codigo.ToString("D3"));
                header.Append("0000");
                header.Append("0");
                header.Append(Utils.FormatCode("", " ", 9));

                header.Append(cedente.CPFCNPJ.Length == 11 ? "1" : "2");
                header.Append(Utils.FormatCode(cedente.CPFCNPJ, "0", 14, true));
                header.Append(Utils.FormatCode("", " ", 20));

                header.Append(Utils.FormatCode(cedente.ContaBancaria?.Agencia, "0", 5, true));
                header.Append(" ");
                header.Append(Utils.FormatCode(cedente.ContaBancaria?.Conta, "0", 12, true));
                header.Append(string.IsNullOrEmpty(cedente.ContaBancaria?.DigitoConta) ? " " : cedente.ContaBancaria.DigitoConta);

                header.Append(" ");
                header.Append(Utils.FitStringLength(cedente.Nome, 30, 30, ' ', 0, true, true, false));
                header.Append(Utils.FormatCode("Banco Safra S/A", " ", 30));
                header.Append(Utils.FormatCode("", " ", 10));

                header.Append("1");
                header.Append(DateTime.Now.ToString("ddMMyyyy"));
                header.Append("000000");
                header.Append(numeroArquivoRemessa.ToString("D6"));
                header.Append("103");
                header.Append("00000");

                header.Append(Utils.FormatCode("", " ", 20));
                header.Append(Utils.FormatCode("", " ", 20));
                header.Append(Utils.FormatCode("", " ", 29));

                return Utils.SubstituiCaracteresEspeciais(header.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do arquivo de remessa do CNAB240.", ex);
            }
        }

        public override string GerarHeaderLoteRemessa(string numeroConvenio, Cedente cedente, int numeroArquivoRemessa, TipoArquivo tipoArquivo)
        {
            try
            {
                string header = " ";

                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        header = GerarHeaderLoteRemessaCNAB240(cedente, numeroArquivoRemessa);
                        break;
                    case TipoArquivo.CNAB400:
                        throw new NotImplementedException("Remessa não implementada!");
                    case TipoArquivo.Outro:
                        throw new Exception("Tipo de arquivo inexistente.");
                }

                return header;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a geração do HEADER DO LOTE do arquivo de REMESSA.", ex);
            }
        }

        private string GerarHeaderLoteRemessaCNAB240(Cedente cedente, int numeroArquivoRemessa)
        {
            try
            {
                StringBuilder header = new StringBuilder(240);

                header.Append(Codigo.ToString("D3")); 
                
                header.Append("0001");
                header.Append("1");
                header.Append("R");
                header.Append("01");
                header.Append("  ");
                header.Append("060");
                header.Append(" ");

                header.Append(cedente.CPFCNPJ.Length == 11 ? "1" : "2");
                header.Append(Utils.FormatCode(cedente.CPFCNPJ, "0", 15, true));
                header.Append(Utils.FormatCode("", " ", 20));

                header.Append(Utils.FormatCode(cedente.ContaBancaria?.Agencia, "0", 5, true));
                header.Append(" ");
                header.Append(Utils.FormatCode(cedente.ContaBancaria?.Conta, "0", 12, true));
                header.Append(string.IsNullOrEmpty(cedente.ContaBancaria?.DigitoConta) ? " " : cedente.ContaBancaria.DigitoConta);
                header.Append(" ");

                header.Append(Utils.FitStringLength(cedente.Nome, 30, 30, ' ', 0, true, true, false));

                header.Append(Utils.FormatCode("", " ", 40));
                header.Append(Utils.FormatCode("", " ", 40));

                header.Append(Utils.FormatCode(numeroArquivoRemessa.ToString(), "0", 8, true));

                header.Append(DateTime.Now.ToString("ddMMyyyy"));
                header.Append(Utils.FormatCode("", "0", 8, true));
                header.Append(Utils.FormatCode("", " ", 33));

                return Utils.SubstituiCaracteresEspeciais(header.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao gerar HEADER DO LOTE do arquivo de remessa.", e);
            }
        }

        public override string GerarDetalheSegmentoPRemessa(Boleto boleto, int numeroRegistro, string numeroConvenio)
        {
            try
            {
                string CodJurosMora = boleto.CodJurosMora;
                if (string.IsNullOrEmpty(CodJurosMora))
                {
                    if (boleto.JurosMora == 0 && boleto.PercJurosMora == 0)
                    {
                        CodJurosMora = "3";
                    }
                    else
                    {
                        CodJurosMora = "1";
                    }
                }

                string vInstrucao1 = "0";
                string vInstrucao2 = "00";
                foreach (IInstrucao instrucao in boleto.Instrucoes)
                {
                    switch ((EnumInstrucoes_Safra)instrucao.Codigo)
                    {
                        case EnumInstrucoes_Safra.ProtestarAposNDiasCorridos:
                        case EnumInstrucoes_Safra.ProtestarAposNDiasUteis:
                            vInstrucao1 = Utils.FitStringLength(instrucao.Codigo.ToString(), 1, 1, '0', 0, true, true, true);
                            vInstrucao2 = Utils.FitStringLength(instrucao.QuantidadeDias.ToString(), 2, 2, '0', 0, true, true, true);
                            break;
                    }
                }

                StringBuilder segmentoP = new StringBuilder(240);

                segmentoP.Append(Codigo.ToString("D3"));
                segmentoP.Append("0001");
                segmentoP.Append("3");
                segmentoP.Append(Utils.FitStringLength(numeroRegistro.ToString(), 5, 5, '0', 0, true, true, true));
                segmentoP.Append("P");
                segmentoP.Append(" ");
                segmentoP.Append(Utils.FormatCode(((int)TipoOcorrenciaRemessa.EntradaDeTitulos).ToString(), 2));

                segmentoP.Append(Utils.FitStringLength(boleto.Cedente.ContaBancaria.Agencia, 5, 5, '0', 0, true, true, true));
                segmentoP.Append(" ");
                segmentoP.Append(Utils.FitStringLength(boleto.Cedente.ContaBancaria.Conta, 12, 12, '0', 0, true, true, true));
                segmentoP.Append(Utils.FormatCode(string.IsNullOrEmpty(boleto.Cedente.ContaBancaria.DigitoConta) ? " " : boleto.Cedente.ContaBancaria.DigitoConta, " ", 1, true));
                segmentoP.Append(" ");

                segmentoP.Append(Utils.FitStringLength(boleto.NossoNumero, 9, 9, '0', 0, true, true, true));
                segmentoP.Append(Utils.FitStringLength(boleto.DigitoNossoNumero, 1, 1, '0', 1, true, true, true));
                segmentoP.Append(Utils.FormatCode("", " ", 10));

                segmentoP.Append(Convert.ToInt16(boleto.Carteira) == 1 ? "1" : "2");
                segmentoP.Append("1");
                segmentoP.Append("2");
                segmentoP.Append("2");
                segmentoP.Append("2");

                segmentoP.Append(Utils.FitStringLength(boleto.NumeroDocumento, 10, 10, ' ', 0, true, true, false));
                segmentoP.Append("00000");
                segmentoP.Append(Utils.FitStringLength(boleto.DataVencimento.ToString("ddMMyyyy"), 8, 8, ' ', 0, true, true, false));
                segmentoP.Append(Utils.FitStringLength(boleto.ValorBoleto.ApenasNumeros(), 15, 15, '0', 0, true, true, true));
                segmentoP.Append(Utils.FitStringLength(boleto.Cedente.ContaBancaria.Agencia, 5, 5, '0', 0, true, true, true));
                segmentoP.Append(" ");
                segmentoP.Append(Utils.FitStringLength(boleto.EspecieDocumento.Codigo.ToString(), 2, 2, '0', 0, true, true, true));
                segmentoP.Append("N");
                segmentoP.Append(Utils.FormatCode(boleto.DataProcessamento.ToString("ddMMyyyy"), 8));

                segmentoP.Append(Utils.FormatCode(CodJurosMora, 1));
                segmentoP.Append(Utils.FitStringLength(boleto.DataJurosMora.ToString("ddMMyyyy"), 8, 8, ' ', 0, true, true, false));
                segmentoP.Append(Utils.FitStringLength(boleto.JurosMora.ApenasNumeros(), 15, 15, '0', 0, true, true, true));
                segmentoP.Append("1");

                if (boleto.DataDesconto > DateTime.MinValue)
                {
                    segmentoP.Append(Utils.FormatCode(boleto.DataDesconto.ToString("ddMMyyyy"), 8));
                    segmentoP.Append(Utils.FormatCode(boleto.ValorDesconto.ToString("f").Replace(",", "").Replace(".", ""), 15));
                }
                else
                {
                    segmentoP.Append(Utils.FormatCode("", "0", 8));
                    segmentoP.Append(Utils.FormatCode("", "0", 15));
                }

                segmentoP.Append(Utils.FormatCode(boleto.IOF.ToString(), 15));
                segmentoP.Append(Utils.FormatCode("", "0", 15));
                segmentoP.Append(Utils.FormatCode(boleto.NumeroDocumento, " ", 25));

                segmentoP.Append(Utils.FormatCode(vInstrucao1, 1));
                segmentoP.Append(Utils.FormatCode(vInstrucao2, 2));

                segmentoP.Append("2");
                segmentoP.Append("   ");

                segmentoP.Append("09");
                segmentoP.Append("0000000000");
                segmentoP.Append("1");

                return Utils.SubstituiCaracteresEspeciais(segmentoP.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a geração do SEGMENTO P DO DETALHE do arquivo de REMESSA.", ex);
            }
        }

        public override string GerarDetalheSegmentoQRemessa(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                StringBuilder segmentoQ = new StringBuilder(240);

                segmentoQ.Append(Codigo.ToString("D3"));
                segmentoQ.Append("0001");
                segmentoQ.Append("3");
                segmentoQ.Append(Utils.FitStringLength(numeroRegistro.ToString(), 5, 5, '0', 0, true, true, true));
                segmentoQ.Append("Q");
                segmentoQ.Append(" ");
                segmentoQ.Append(Utils.FormatCode(((int)TipoOcorrenciaRemessa.EntradaDeTitulos).ToString(), 2));

                segmentoQ.Append(boleto.Sacado.CPFCNPJ.Length == 11 ? "1" : "2");
                segmentoQ.Append(Utils.FormatCode(boleto.Sacado.CPFCNPJ, "0", 15, true));
                segmentoQ.Append(Utils.FitStringLength(boleto.Sacado.Nome.TrimStart(' '), 40, 40, ' ', 0, true, true, false).ToUpper());
                segmentoQ.Append(Utils.FitStringLength(boleto.Sacado.Endereco.End.TrimStart(' '), 40, 40, ' ', 0, true, true, false).ToUpper());
                segmentoQ.Append(Utils.FitStringLength(boleto.Sacado.Endereco.Bairro.TrimStart(' '), 15, 15, ' ', 0, true, true, false).ToUpper());
                segmentoQ.Append(Utils.FitStringLength(boleto.Sacado.Endereco.CEP, 8, 8, ' ', 0, true, true, false).ToUpper());
                segmentoQ.Append(Utils.FitStringLength(boleto.Sacado.Endereco.Cidade.TrimStart(' '), 15, 15, ' ', 0, true, true, false).ToUpper());
                segmentoQ.Append(Utils.FitStringLength(boleto.Sacado.Endereco.UF, 2, 2, ' ', 0, true, true, false).ToUpper());

                segmentoQ.Append(boleto.Cedente.CPFCNPJ.Length == 11 ? "1" : "2");
                segmentoQ.Append(Utils.FormatCode(boleto.Cedente.CPFCNPJ, "0", 15, true));
                segmentoQ.Append(Utils.FitStringLength(boleto.Cedente.Nome.TrimStart(' '), 40, 40, ' ', 0, true, true, false).ToUpper());

                segmentoQ.Append("000");
                segmentoQ.Append(Utils.FormatCode("", " ", 20));
                segmentoQ.Append(Utils.FormatCode("", " ", 8));

                return Utils.SubstituiCaracteresEspeciais(segmentoQ.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a geração do SEGMENTO Q DO DETALHE do arquivo de REMESSA.", ex);
            }
        }

        public override string GerarDetalheSegmentoRRemessa(Boleto boleto, int numeroRegistro, TipoArquivo tipoArquivo)
        {
            try
            {
                StringBuilder segmentoR = new StringBuilder(240);

                segmentoR.Append(Codigo.ToString("D3"));
                segmentoR.Append("0001");
                segmentoR.Append("3");
                segmentoR.Append(Utils.FitStringLength(numeroRegistro.ToString(), 5, 5, '0', 0, true, true, true));
                segmentoR.Append("R");
                segmentoR.Append(" ");
                segmentoR.Append(Utils.FormatCode(((int)TipoOcorrenciaRemessa.EntradaDeTitulos).ToString(), 2));

                segmentoR.Append("0");
                segmentoR.Append(Utils.FormatCode("", "0", 8));
                segmentoR.Append(Utils.FormatCode("", "0", 15));
                segmentoR.Append("0");
                segmentoR.Append(Utils.FormatCode("", "0", 8));
                segmentoR.Append(Utils.FormatCode("", "0", 15));

                segmentoR.Append("2");
                segmentoR.Append(Utils.FitStringLength(boleto.DataMulta.ToString("ddMMyyyy"), 8, 8, '0', 0, true, true, false));
                segmentoR.Append(Utils.FitStringLength(boleto.ValorMulta.ApenasNumeros(), 15, 15, '0', 0, true, true, true));

                segmentoR.Append(Utils.FormatCode("", " ", 10));
                segmentoR.Append(Utils.FormatCode("", " ", 40));
                segmentoR.Append(Utils.FormatCode("", " ", 40));
                segmentoR.Append(Utils.FormatCode("", " ", 20));
                segmentoR.Append(Utils.FormatCode("", " ", 8));
                segmentoR.Append(Utils.FormatCode("", " ", 3));
                segmentoR.Append(Utils.FormatCode("", " ", 5));
                segmentoR.Append(" ");
                segmentoR.Append(Utils.FormatCode("", " ", 12));
                segmentoR.Append(" ");
                segmentoR.Append(" ");
                segmentoR.Append(" ");
                segmentoR.Append(Utils.FormatCode("", " ", 9));

                return Utils.SubstituiCaracteresEspeciais(segmentoR.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a geração do SEGMENTO R DO DETALHE do arquivo de REMESSA.", ex);
            }
        }

        public override string GerarTrailerLoteRemessa(int numeroRegistro)
        {
            try
            {
                StringBuilder header = new StringBuilder(240);

                header.Append(Codigo.ToString("D3")); 
                
                header.Append("0001");
                header.Append("5");
                header.Append(Utils.FormatCode("", " ", 9));
                header.Append(Utils.FormatCode(numeroRegistro.ToString(), "0", 6, true));

                header.Append(Utils.FormatCode("", "0", 6));
                header.Append(Utils.FormatCode("", "0", 17));

                header.Append(Utils.FormatCode("", "0", 6));
                header.Append(Utils.FormatCode("", "0", 17));

                header.Append(Utils.FormatCode("", "0", 6));
                header.Append(Utils.FormatCode("", "0", 17));
                header.Append(Utils.FormatCode("", "0", 6));

                header.Append(Utils.FormatCode("", " ", 17));
                header.Append(Utils.FormatCode("", " ", 8));
                header.Append(Utils.FormatCode("", " ", 117));

                return Utils.SubstituiCaracteresEspeciais(header.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao gerar Trailer de Lote do arquivo de remessa.", e);
            }
        }

        public override string GerarTrailerArquivoRemessa(int numeroRegistro)
        {
            try
            {
                StringBuilder header = new StringBuilder(240);

                header.Append(Codigo.ToString("D3"));

                header.Append("9999");
                header.Append("9");
                header.Append(Utils.FormatCode("", " ", 9));
                header.Append("000001");
                header.Append(Utils.FormatCode(numeroRegistro.ToString(), "0", 6, true));
                header.Append(Utils.FormatCode("", "0", 6));
                header.Append(Utils.FormatCode("", " ", 205));

                return Utils.SubstituiCaracteresEspeciais(header.ToString());
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao gerar Trailer de arquivo de remessa.", e);
            }
        }

        public override void ValidaBoleto(Boleto boleto)
        {

            // Calcula o DAC do Nosso Número
            _dacNossoNumero = CalcularDigitoNossoNumero(boleto);

            // Calcula o DAC da Conta Corrente
            _dacContaCorrente = Mod10(boleto.Cedente.ContaBancaria.Agencia + boleto.Cedente.ContaBancaria.Conta);

            //Verifica se o nosso número é válido
            if (Utils.ToInt64(boleto.NossoNumero) == 0)
                throw new NotImplementedException("Nosso número inválido");

            //Verifica se data do processamento é valida
			//if (boleto.DataProcessamento.ToString("dd/MM/yyyy") == "01/01/0001")
			if (boleto.DataProcessamento == DateTime.MinValue) // diegomodolo (diego.ribeiro@nectarnet.com.br)
                boleto.DataProcessamento = DateTime.Now;

            //Verifica se data do documento é valida
			//if (boleto.DataDocumento.ToString("dd/MM/yyyy") == "01/01/0001")
			if (boleto.DataDocumento == DateTime.MinValue) // diegomodolo (diego.ribeiro@nectarnet.com.br)
                boleto.DataDocumento = DateTime.Now;

            FormataCodigoBarra(boleto);
            FormataLinhaDigitavel(boleto);
            FormataNossoNumero(boleto);
        }

        public override void FormataNumeroDocumento(Boleto boleto)
        {
            throw new NotImplementedException("Função não implementada.");
        }

        public override void FormataNossoNumero(Boleto boleto)
        {
            //throw new NotImplementedException("Função não implementada.");
        }

        /// <summary>
        ///	O código de barra para cobrança contém 44 posições dispostas da seguinte forma:
        ///    01 a 03 - 3 - Identificação  do  Banco
        ///    04 a 04 - 1 - Código da Moeda
        ///    05 a 05 – 1 - Dígito verificador do Código de Barras
        ///    06 a 19 - 14 - Valor
        ///    20 a 44 – 25 - Campo Livre
        /// </summary>
        public override void FormataCodigoBarra(Boleto boleto)
        {
            string valorBoleto = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
            valorBoleto = Utils.FormatCode(valorBoleto, 14);

            boleto.CodigoBarra.Codigo = string.Format("{0}{1}{2}{3}{4}", Codigo, boleto.Moeda,
                    FatorVencimento(boleto), valorBoleto, CampoLivre(boleto));

            _dacBoleto = 0;
            //Mod11(Boleto.CodigoBarra.Codigo.Substring(0, 3) + Boleto.CodigoBarra.Codigo.Substring(5, 43), 9, 0);

            boleto.CodigoBarra.Codigo = Strings.Left(boleto.CodigoBarra.Codigo, 4) + _dacBoleto + Strings.Right(boleto.CodigoBarra.Codigo, 39);
        }

        /// <summary>
        /// A linha digitável será composta por cinco campos:
        ///    1º CAMPO - Composto pelo código do banco ( sem o dígito verificador = 422 ), 
        ///       código da moeda, as cinco primeiras posições do campo livre ou seja, da 
        ///       posição 20 à 24 do código de barras, e mais um dígito verificador deste campo. 
        ///       Após os 5 primeiros dígitos deste campo separar o conteúdo por um ponto ( . ). 
        ///    2º CAMPO - Composto pelas posições 6 à 15 do campo livre ou seja, da 
        ///       posição 25 à 34 do código de barras e mais um dígito verificador deste campo. 
        ///       Após os 5 primeiros dígitos deste campo separar o conteúdo por um ponto ( . ).
        ///    3º CAMPO - Composto pelas posições 16 à 25 do campo livre ou seja, da 
        ///       posição 35 à 44 do código de barras, e mais um dígito verificador deste campo. 
        ///       Após os 5 primeiros dígitos deste campo separar o conteúdo por um ponto ( . ).
        ///    4º CAMPO  - Composto pelo dígito de autoconferência do código de barras.
        ///    5º CAMPO - Composto pelo valor nominal do documento ou seja, pelas 
        ///       posições 06 à 19 do código de barras, com supressão de zeros a esquerda e 
        ///       sem edição ( sem ponto e vírgula ). Quando se tratar de valor zerado, a 
        ///       representação deverá ser 000 ( três zeros ).
        /// </summary>
        public override void FormataLinhaDigitavel(Boleto boleto)
        {

            //AAABC.CCCCX DDDDD.DDDDDY EEEEE.EEEEEZ K VVVVVVVVVVVVVV

            string LD = string.Empty; //Linha Digitável

            #region Campo 1

            //Campo 1
            string AAA = Utils.FormatCode(Codigo.ToString(), 3);
            string B = boleto.Moeda.ToString();
            string CCCCC = CampoLivre(boleto).Substring(0, 4);
            string X = Mod10(AAA + B + CCCCC).ToString();

            LD = string.Format("{0}{1}{2}.", AAA, B, CCCCC.Substring(0, 1));
            LD += string.Format("{0}{1}", CCCCC.Substring(0, 4), X);

            #endregion Campo 1

            #region Campo 2

            string DDDDDD = CampoLivre(boleto).Substring(6, 15);
            string Y = Mod10(DDDDDD).ToString();

            LD += string.Format("{0}.", DDDDDD.Substring(0, 5));
            LD += string.Format("{0}{1} ", DDDDDD.Substring(5, 10), Y);

            #endregion Campo 2

            #region Campo 3

            string EEEEE = CampoLivre(boleto).Substring(12, 10);
            string Z = Mod10(EEEEE).ToString();

            LD += string.Format("{0}.", EEEEE.Substring(0, 5));
            LD += string.Format("{0}{1} ", EEEEE.Substring(5, 5), Z);

            #endregion Campo 3

            #region Campo 4

            string K = _dacBoleto.ToString();

            LD += string.Format(" {0} ", K);

            #endregion Campo 4

            #region Campo 5
            string VVVVVVVVVVVVVV;
            if (boleto.ValorBoleto != 0)
            {
                VVVVVVVVVVVVVV = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
                VVVVVVVVVVVVVV = Utils.FormatCode(VVVVVVVVVVVVVV, 14);
            }
            else
                VVVVVVVVVVVVVV = "000";

            LD += VVVVVVVVVVVVVV;

            #endregion Campo 5

            boleto.CodigoBarra.LinhaDigitavel = LD;

        }
        #endregion IBanco Members


        /// <summary>
        /// Efetua as Validações dentro da classe Boleto, para garantir a geração da remessa
        /// </summary>
        public override bool ValidarRemessa(TipoArquivo tipoArquivo, string numeroConvenio, IBanco banco, Cedente cedente, Boletos boletos, int numeroArquivoRemessa, out string mensagem)
        {
            bool vRetorno = true;
            string vMsg = string.Empty;
            ////IMPLEMENTACAO PENDENTE...
            mensagem = vMsg;
            return vRetorno;
        }


        public override DetalheSegmentoTRetornoCNAB240 LerDetalheSegmentoTRetornoCNAB240(string registro)
        {
            try
            {
                DetalheSegmentoTRetornoCNAB240 detalhe = new DetalheSegmentoTRetornoCNAB240(registro);

                if (registro.Substring(13, 1) != "T")
                {
                    throw new Exception("Registro inválido. O detalhe não possuí as características do segmento T.");
                }

                detalhe.CodigoBanco = Convert.ToInt32(registro.Substring(0, 3));
                detalhe.idCodigoMovimento = Convert.ToInt32(registro.Substring(15, 2));
                detalhe.Agencia = Convert.ToInt32(registro.Substring(17, 5));
                detalhe.DigitoAgencia = registro.Substring(22, 1);
                detalhe.Conta = Convert.ToInt32(registro.Substring(23, 12));
                detalhe.DigitoConta = registro.Substring(35, 1);
                detalhe.NossoNumero = registro.Substring(37, 20);
                detalhe.CodigoCarteira = Convert.ToInt32(registro.Substring(57, 1));
                detalhe.NumeroDocumento = registro.Substring(58, 15);
                int dataVencimento = Convert.ToInt32(registro.Substring(73, 8));
                detalhe.DataVencimento = Convert.ToDateTime(dataVencimento.ToString("##-##-####"));
                decimal valorTitulo = Convert.ToInt64(registro.Substring(81, 15));
                detalhe.ValorTitulo = valorTitulo / 100;
                detalhe.IdentificacaoTituloEmpresa = registro.Substring(105, 25);
                detalhe.TipoInscricao = Convert.ToInt32(registro.Substring(132, 1));
                detalhe.NumeroInscricao = registro.Substring(133, 15);
                detalhe.NomeSacado = registro.Substring(148, 40);
                decimal valorTarifas = Convert.ToUInt64(registro.Substring(198, 15));
                detalhe.ValorTarifas = valorTarifas / 100;

                return detalhe;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao processar arquivo de RETORNO - SEGMENTO T.", ex);
            }


        }
    }
}
