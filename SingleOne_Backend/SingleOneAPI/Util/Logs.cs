using System;
using System.IO;

namespace SingleOne.Util
{
    public class Logs
    {
        //public static void Logar(Exception ex, string caminhoLog, string usuario = "")
        //{
        //    //string arquivo = caminhoLog + DateTime.Today.ToString("dd-MM-yyyy") + ".log";
        //    //if (!Directory.Exists(caminhoLog))
        //    //    Directory.CreateDirectory(caminhoLog);

        //    //StreamWriter sw;
        //    //if (!File.Exists(Path.Combine(caminho, arquivo)))
        //    //    sw = new StreamWriter(Path.Combine(caminho, arquivo));
        //    //else

        //    //    sw = new StreamWriter(Path.Combine(caminho, arquivo), append: true);
        //    //if (!File.Exists(arquivo))
        //    //    sw = new StreamWriter(arquivo);
        //    //else
        //    //    sw = new StreamWriter(arquivo, append: true);

        //    //sw.WriteLine("-------------------------------------------------------------------------------------------------------------------");
        //    //sw.WriteLine("Em: " + TimeZoneMapper.GetDateTimeNow().ToString("dd/MM/yyyy HH:mm:ss"));
        //    //if (!String.IsNullOrWhiteSpace(usuario))
        //    //    sw.WriteLine("Por: " + usuario);
        //    //sw.WriteLine("Erro: " + ex.Message);
        //    //sw.WriteLine("Stack Trace: " + ex.StackTrace);
        //    //sw.WriteLine("Source: " + ex.Source);

        //    //sw.Flush();
        //    //sw.Close();
        //}

        //public static void Logar(string mensagem, string caminhoLog, string usuario = "")
        //{
        //    string arquivo = caminhoLog + DateTime.Today.ToString("dd-MM-yyyy") + ".log";
        //    if (!Directory.Exists(caminhoLog))
        //        Directory.CreateDirectory(caminhoLog);

        //    StreamWriter sw;
        //    //if (!File.Exists(Path.Combine(caminho, arquivo)))
        //    //    sw = new StreamWriter(Path.Combine(caminho, arquivo));
        //    //else

        //    //    sw = new StreamWriter(Path.Combine(caminho, arquivo), append: true);
        //    if (!File.Exists(arquivo))
        //        sw = new StreamWriter(arquivo);
        //    else
        //        sw = new StreamWriter(arquivo, append: true);

        //    sw.WriteLine("-------------------------------------------------------------------------------------------------------------------");
        //    sw.WriteLine("Em: " + TimeZoneMapper.GetDateTimeNow().ToString("dd/MM/yyyy HH:mm:ss"));
        //    if (!String.IsNullOrWhiteSpace(usuario))
        //        sw.WriteLine("Por: " + usuario);
        //    sw.WriteLine("Mensagem: " + mensagem);

        //    sw.Flush();
        //    sw.Close();
        //}
    }
}
