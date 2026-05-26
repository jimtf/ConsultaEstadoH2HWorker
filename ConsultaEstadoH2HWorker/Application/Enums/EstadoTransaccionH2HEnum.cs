namespace ConsultaEstadoH2HWorker.Application.Enums
{
    public class EstadoTransaccionH2HEnum
    {
        public enum EstadoTransaccionH2H
        {
            Exito = 1,
            Error = 2,
            Exito_Parcial = 3,
            Pendiente_Procesar = 4,
            Registrado = 11,
            Pendiente = 12,
        }
    }
}
