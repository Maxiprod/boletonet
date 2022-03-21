using System;
using System.Collections.Generic;
using System.Text;

namespace BoletoNet
{
    #region Enumerado

    public enum EnumInstrucoes_Safra
    {
        ProtestarAposNDiasCorridos = 1,
        ProtestarAposNDiasUteis = 2,
        NaoProtestar = 3,
    }

    #endregion

    public class Instrucao_Safra : AbstractInstrucao, IInstrucao
    {
        #region Construtores

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
            this.carregar(codigo, 0);
        }

        public Instrucao_Safra(int codigo, int nrDias)
        {
            this.carregar(codigo, nrDias);
        }
        #endregion Construtores

        #region Metodos Privados

        private void carregar(int idInstrucao, int nrDias)
        {
            try
            {
                this.Banco = new Banco_Safra();
                this.Valida();

                switch ((EnumInstrucoes_Safra)idInstrucao)
                {
                    case EnumInstrucoes_Safra.ProtestarAposNDiasCorridos:
                        this.Codigo = (int)EnumInstrucoes_Safra.ProtestarAposNDiasCorridos;
                        this.Descricao = "PROTESTAR APÓS " + nrDias + " DIAS CORRIDOS DO VENCIMENTO";
                        break;
                    case EnumInstrucoes_Safra.ProtestarAposNDiasUteis:
                        this.Codigo = (int)EnumInstrucoes_Safra.ProtestarAposNDiasUteis;
                        this.Descricao = "PROTESTAR APÓS " + nrDias + " DIAS ÚTEIS DO VENCIMENTO";
                        break;
                    case EnumInstrucoes_Safra.NaoProtestar:
                        this.Codigo = (int)EnumInstrucoes_Safra.NaoProtestar;
                        this.Descricao = "Não protestar";
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

        #endregion

    }
}
