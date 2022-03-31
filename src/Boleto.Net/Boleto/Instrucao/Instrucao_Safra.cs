using System;

namespace BoletoNet
{
    #region Enumerado

    public enum EnumInstrucoes_Safra
    {
        ProtestarAposNDiasCorridos = 1,
        ProtestarAposNDiasUteis = 2,
        NaoProtestar = 3,
        MoraDiaria = 900,
        MultaVencimento = 901
    }

    #endregion

    public class Instrucao_Safra : AbstractInstrucao, IInstrucao
    {
        public Instrucao_Safra()
        {
            try
            {
                this.Banco = new Banco(422);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao carregar objeto", ex);
            }
        }

        public Instrucao_Safra(int codigo)
        {
            this.carregar(codigo, 0, 0);
        }

        public Instrucao_Safra(int codigo, int nrDias)
        {
            this.carregar(codigo, nrDias, 0);
        }

        public Instrucao_Safra(int codigo, double valorDaMultaOuJuros)
        {
            this.carregar(codigo, 0, valorDaMultaOuJuros);
        }

        public Instrucao_Safra(int codigo, int nrDias, double valorDaMultaOuJuros)
        {
            this.carregar(codigo, nrDias, valorDaMultaOuJuros);
        }

        private void carregar(int idInstrucao, int nrDias, double valorDaMultaOuJuros)
        {
            try
            {
                this.Banco = new Banco_Safra();
                this.Valida();

                switch ((EnumInstrucoes_Safra)idInstrucao)
                {
                    case EnumInstrucoes_Safra.ProtestarAposNDiasCorridos:
                        this.Codigo = (int)EnumInstrucoes_Safra.ProtestarAposNDiasCorridos;
                        this.Descricao = $"Protestar após {nrDias} dias corridos do vencimento";
                        break;
                    case EnumInstrucoes_Safra.ProtestarAposNDiasUteis:
                        this.Codigo = (int)EnumInstrucoes_Safra.ProtestarAposNDiasUteis;
                        this.Descricao = $"Protestar após {nrDias} dias úteis do vencimento";
                        break;
                    case EnumInstrucoes_Safra.NaoProtestar:
                        this.Codigo = (int)EnumInstrucoes_Safra.NaoProtestar;
                        this.Descricao = "Não protestar";
                        break;
                    case EnumInstrucoes_Safra.MultaVencimento:
                        this.Codigo = (int)EnumInstrucoes_Safra.MultaVencimento;
                        this.Descricao = string.Format("Após vencimento cobrar multa de {0}", valorDaMultaOuJuros.ToString("C"));
                        break;
                    case EnumInstrucoes_Safra.MoraDiaria:
                        this.Codigo = (int)EnumInstrucoes_Safra.MoraDiaria;
                        this.Descricao = string.Format("Após vencimento cobrar juros de {0} por dia de atraso", valorDaMultaOuJuros.ToString("C"));
                        break;
                    default:
                        this.Codigo = 0;
                        this.Descricao = " (Selecione) ";
                        break;
                }

                this.QuantidadeDias = nrDias;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao carregar objeto", ex);
            }
        }

        public override void Valida()
        {
            //base.Valida();
        }
    }
}
