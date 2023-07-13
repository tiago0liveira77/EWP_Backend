namespace EWP_API_WEB_APP.Utilities.Utils
{
    public class FileUtils
    {

        const string defaultImageFileName = "default-image.jpg";

        public static string addImageToServer(IFormFile Ficheiro, ILogger _logger, string uploadPath)
        {
            // Processar o ficheiro
            // Verificar se foi preenchido
            if (Ficheiro != null)
            {
                _logger.LogWarning("Inicio do processo do ficheiro: " + Ficheiro.FileName);
                string nomeFicheiro = Ficheiro.FileName;

                if (!Directory.Exists(uploadPath)) //Verifica se a diretória existe, senão cria uma
                {
                    _logger.LogWarning(uploadPath + " não existe.");
                    Directory.CreateDirectory(uploadPath);
                }

                string filePath = Path.Combine(uploadPath, nomeFicheiro);  //Controi o caminho do ficheiro

                if (File.Exists(filePath))
                { //Se já existir um ficheiro com o mesmo nome
                    _logger.LogWarning("Ficheiro já existente.");
                    while (File.Exists(filePath)) //Enquanto for encontrado um ficheiro com o mesmo nome
                    {
                        /**
                         * É gerado um identificador unico para a imagem baseados em dois conceitos:
                         * Nome da imagem + A hora local do sistema formatada da seguinte forma "yyyyMMddHHmmss" + um GUID gerado automaticamente de 0 a 4.
                         */
                        string identifier = DateTime.Now.ToString("yyyyMMddHHmmss") + TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes.ToString("+#;-#") + Guid.NewGuid().ToString().Substring(0, 4);
                        string uniqueFileName = Path.GetFileNameWithoutExtension(nomeFicheiro) + "_" + identifier + Path.GetExtension(nomeFicheiro);
                        filePath = Path.Combine(uploadPath, uniqueFileName);
                        nomeFicheiro = uniqueFileName;
                        _logger.LogWarning("Novo ficheiro gerado: " + uniqueFileName);
                    }
                }
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    _logger.LogWarning("Ficheiro copiado para a diretoria.");
                    Ficheiro.CopyTo(fileStream);
                }
                return nomeFicheiro;

            }
            else
            {
                //Ficheiro não foi submetido, irá ser carregado um default.
                _logger.LogWarning("Não foi carregado nenhum ficheiro, irá ser assumido o ficheiro default: " + defaultImageFileName);
                return defaultImageFileName;
            }
        }

    }
}
