using System;
using System.IO;
using System.IO.Ports;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

class Program
{
    static async Task Main(string[] args)
    {
        //Configuracao configuracao = CarregarConfiguracoes(@"C:\Users\gabriel.sousa_siteme\Desktop\robo_final\config.json");
        string caminhoPastaExecutavel = AppDomain.CurrentDomain.BaseDirectory;
        string caminhoConfig = Path.Combine(caminhoPastaExecutavel, "config.json");
        Configuracao configuracao = CarregarConfiguracoes(caminhoConfig);

        bool executarEsperarAteLiberado = true;

        async Task EsperarAteLiberado(SerialPort port, string liberado, TimeSpan delay)
        {
            port.WriteLine(configuracao.StatusRobot);
            Console.WriteLine("Enviando comando para verificar status...");

            while (true)
            {
                // Aguardar um pouco antes de ler a resposta
                await Task.Delay(delay);

                // Ler a resposta
                string resposta = port.ReadLine();
                Console.WriteLine(resposta);

                // Verificar se a resposta contém a palavra-chave "ok"
                if (resposta.Contains(configuracao.ReturnStatusRobot))
                {
                    Console.WriteLine("Liberado!");
                    break;  // Sair do loop se "ok" for encontrado
                }
            }
        }


        Dictionary<string, string?> positionMapping = new Dictionary<string, string?> {
        { "T0", configuracao.ValueT0 },
        { "T1", configuracao.ValueT1 },
        { "T2", configuracao.ValueT2 },
        { "T3", configuracao.ValueT3 },
        { "T4", configuracao.ValueT4 },
        { "T5", configuracao.ValueT5 },
        { "T6", configuracao.ValueT6 },
        { "T7", configuracao.ValueT7 },
        { "T8", configuracao.ValueT8 },
        { "P0", configuracao.ValueP0 },
        { "P1", configuracao.ValueP1 },
        { "P2", configuracao.ValueP2 },
        { "P3", configuracao.ValueP3 },
        { "P4", configuracao.ValueP4 },
        { "DropPiece", configuracao.DropPiece },
        { "PushPiece", configuracao.PushPiece },
        { "InitialPositionZ", configuracao.InitialPositionZ },
        { "InitialPositionX", configuracao.InitialPositionX },
        { "InitialPositionY", configuracao.InitialPositionY },
        { "AbsolutePosition", configuracao.AbsolutePosition },
        { "FinalPosition", configuracao.FinalPosition },
        { "GoDown", configuracao.GoDown },
        { "UpWithPiece", configuracao.UpWithPiece },
        { "GoDownWithPiece", configuracao.GoDownWithPiece },
        { "UpWithoutPiece", configuracao.UpWithoutPiece },};

        Dictionary<string, string?> positionInvertedMapping = new Dictionary<string, string?> {
        { "T0", configuracao.ValueT8 },
        { "T1", configuracao.ValueT7 },
        { "T2", configuracao.ValueT6 },
        { "T3", configuracao.ValueT5 },
        { "T4", configuracao.ValueT4 },
        { "T5", configuracao.ValueT3 },
        { "T6", configuracao.ValueT2 },
        { "T7", configuracao.ValueT1 },
        { "T8", configuracao.ValueT0 },
        { "P0", configuracao.ValueP0 },
        { "P1", configuracao.ValueP1 },
        { "P2", configuracao.ValueP2 },
        { "P3", configuracao.ValueP3 },
        { "P4", configuracao.ValueP4 },
        { "DropPiece", configuracao.DropPiece },
        { "PushPiece", configuracao.PushPiece },
        { "InitialPositionZ", configuracao.InitialPositionZ },
        { "InitialPositionX", configuracao.InitialPositionX },
        { "InitialPositionY", configuracao.InitialPositionY },
        { "AbsolutePosition", configuracao.AbsolutePosition },
        { "FinalPosition", configuracao.FinalPosition },
        { "GoDown", configuracao.GoDown },
        { "UpWithPiece", configuracao.UpWithPiece },
        { "GoDownWithPiece", configuracao.GoDownWithPiece },
        { "UpWithoutPiece", configuracao.UpWithoutPiece },};

        using (SerialPort serialPort = new SerialPort(configuracao.serialPortName))
        {
            serialPort.BaudRate = configuracao.baudRate;
            serialPort.Encoding = Encoding.UTF8;
            serialPort.NewLine = "\n";
            serialPort.Open();


            using (SerialPort serialPortInverted = new SerialPort(configuracao.serialPortNameInverted))
            {
                serialPortInverted.BaudRate = configuracao.baudRate;
                serialPortInverted.Encoding = Encoding.UTF8;
                serialPortInverted.NewLine = "\n";
                serialPortInverted.Open();

                string initialPositionZ = "InitialPositionZ";
                string initialPositionX = "InitialPositionX";
                string initialPositionY = "InitialPositionY";
                string AbsolutePosition = "AbsolutePosition";
                string finalPosition = "FinalPosition";

                if (configuracao.ActiveFirstSend)
                {
                    // Enviar comandos iniciais para serialPortName
                    foreach (var comando in new[] { initialPositionZ, initialPositionX, initialPositionY, AbsolutePosition, finalPosition })
                    {
                        string selectedPosition = positionMapping[comando];

                        if (executarEsperarAteLiberado)
                        {
                            await EsperarAteLiberado(serialPort, configuracao.ReturnStatusRobot, TimeSpan.FromSeconds(configuracao.DelaySecondsToSendSerial));
                            serialPort.WriteLine(selectedPosition);
                            Console.WriteLine($"Comando executado com sucesso: {comando}");
                        }
                    }
                }

                if (configuracao.ActiveFirstSend2)
                {
                    // Enviar comandos iniciais para serialPortNameInverted
                    foreach (var comando in new[] { initialPositionZ, initialPositionX, initialPositionY, AbsolutePosition, finalPosition })
                    {
                        string selectedPosition = positionMapping[comando];

                        if (executarEsperarAteLiberado)
                        {
                        await EsperarAteLiberado(serialPortInverted, configuracao.ReturnStatusRobot, TimeSpan.FromSeconds(configuracao.DelaySecondsToSendSerial));
                        serialPortInverted.WriteLine(selectedPosition);
                        Console.WriteLine($"Comando executado com sucesso: {comando}");
                        }
                    }
                }
                while (true)
                {
                    string urlNewGame = configuracao.URLnewGame;
                    string comandNewGame = await NewGame(urlNewGame);

                    if (comandNewGame != null)
                    {
                        respostaNewGame returnId = JsonConvert.DeserializeObject<respostaNewGame>(comandNewGame);
                        string? idNewGame = returnId?.id;
                        Console.WriteLine(comandNewGame);
                        Console.WriteLine(idNewGame);
                        Console.WriteLine(returnId);

                        foreach (var comando in new[] { initialPositionZ, initialPositionX, initialPositionY, AbsolutePosition, finalPosition })
                        {
                            string selectedPosition = positionMapping[comando];

                            if (executarEsperarAteLiberado)
                            {
                                await EsperarAteLiberado(serialPort, configuracao.ReturnStatusRobot, TimeSpan.FromSeconds(configuracao.DelaySecondsToSendSerial));
                                serialPort.WriteLine(selectedPosition);
                                Console.WriteLine($"Comando executado com sucesso: {comando}");
                            }

                            if (executarEsperarAteLiberado)
                            {
                                await EsperarAteLiberado(serialPortInverted, configuracao.ReturnStatusRobot, TimeSpan.FromSeconds(configuracao.DelaySecondsToSendSerial));
                                serialPortInverted.WriteLine(selectedPosition);
                                Console.WriteLine($"Comando executado com sucesso: {comando}");
                            }

                        }
                        Console.WriteLine($"{configuracao.URLnewGameOk}/{idNewGame}");
                        DeleteNewGame(idNewGame, configuracao.URLnewGameOk);


                        while (configuracao.ActiveLoopMovementQuery)
                        {
                            string urlConsultaMovimento = configuracao.UrlQueryMovement;
                            string comandoDaAPI = await LerComandoDaAPI(urlConsultaMovimento);


                            if (comandoDaAPI != null)
                            {
                                var listaResultado = JsonConvert.DeserializeObject<List<RespostaConsultaMovimento>>(comandoDaAPI);

                                foreach (var resultadoConsultaMovimento in listaResultado)
                                {
                                    string player = resultadoConsultaMovimento.player;
                                    string pieceToSend = resultadoConsultaMovimento.piece;
                                    string idToSend = resultadoConsultaMovimento.id;
                                    string positionToSend = resultadoConsultaMovimento.position;
                                    string inverted = resultadoConsultaMovimento.inverted;
                                    string liberado = configuracao.ReturnStatusRobot;
                                    string goDown = "GoDown";
                                    string upWithPiece = "UpWithPiece";
                                    string goDownWithPiece = "GoDownWithPiece";
                                    string dropPiece = "DropPiece";
                                    string pushPiece = "PushPiece";
                                    string upWithoutPiece = "UpWithoutPiece";


                                    if (!string.IsNullOrEmpty(pieceToSend) && !string.IsNullOrEmpty(positionToSend) && inverted == "false")
                                    {

                                        //string[] positions = inverted == "true" ? positionInvertedMapping.Keys.ToArray() : positionMapping.Keys.ToArray();

                                        foreach (string command in new string[] { finalPosition, pieceToSend, pushPiece, goDown, upWithPiece, positionToSend, goDownWithPiece, dropPiece, upWithoutPiece, finalPosition })
                                        {
                                            string selectedPosition;

                                            if (player == "PLAYER_1")
                                            {
                                                await EsperarAteLiberado(serialPortInverted, liberado, TimeSpan.FromSeconds(configuracao.DelaySecondsToSendSerial));
                                                selectedPosition = positionMapping.ContainsKey(command) ? positionMapping[command] : throw new Exception($"Comando não mapeado: {command}");
                                                serialPort.WriteLine(selectedPosition);
                                            }
                                            else
                                            {
                                                await EsperarAteLiberado(serialPortInverted, liberado, TimeSpan.FromSeconds(configuracao.DelaySecondsToSendSerial));
                                                selectedPosition = positionInvertedMapping.ContainsKey(command) ? positionInvertedMapping[command] : throw new Exception($"Comando não mapeado: {command}");
                                                serialPortInverted.WriteLine(selectedPosition);
                                            }

                                            Console.WriteLine($"Comando executado com sucesso: {command}");
                                        }
                                        Console.WriteLine($"{configuracao.UrlRemoveEvent}/{idToSend}");
                                        await Task.Delay(TimeSpan.FromSeconds(configuracao.DelaySecondsMovementQuery));
                                        DeletarDadosNaAPI(idToSend, configuracao.UrlRemoveEvent);
                                    }
                                    else if (!string.IsNullOrEmpty(pieceToSend) && !string.IsNullOrEmpty(positionToSend) && inverted == "true")
                                    {
                                        // Código para inverted == "true"
                                        foreach (string command in new string[] { finalPosition, positionToSend, pushPiece, goDown, upWithPiece, pieceToSend, goDownWithPiece, dropPiece, upWithoutPiece, finalPosition })
                                        {
                                            string selectedPosition;

                                            if (player == "PLAYER_1")
                                            {
                                                await EsperarAteLiberado(serialPort, liberado, TimeSpan.FromSeconds(configuracao.DelaySecondsToSendSerial));
                                                selectedPosition = positionMapping.ContainsKey(command) ? positionMapping[command] : throw new Exception($"Comando não mapeado: {command}");
                                                serialPort.WriteLine(selectedPosition);
                                            }
                                            else
                                            {
                                                await EsperarAteLiberado(serialPortInverted, liberado, TimeSpan.FromSeconds(configuracao.DelaySecondsToSendSerial));
                                                selectedPosition = positionInvertedMapping.ContainsKey(command) ? positionInvertedMapping[command] : throw new Exception($"Comando não mapeado: {command}");
                                                serialPortInverted.WriteLine(selectedPosition);
                                            }

                                            Console.WriteLine($"Comando executado com sucesso: {command}");
                                        }
                                        Console.WriteLine($"{configuracao.UrlRemoveEvent}/{idToSend}");
                                        await Task.Delay(TimeSpan.FromSeconds(configuracao.DelaySecondsMovementQuery));
                                        DeletarDadosNaAPI(idToSend, configuracao.UrlRemoveEvent);

                                    }
                                }

                            }
                            string finalNewGame = await NewGame(urlNewGame);
                            if (finalNewGame == null)
                            {
                                break;
                            }
                        }

                        //await Task.Delay(TimeSpan.FromSeconds(configuracao.DelaySecondsMovementQuery));
                        
                    }
                }
            }
        }

        static async Task<string> LerComandoDaAPI(string urlConsultaMovimento)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(urlConsultaMovimento);

                if (response.IsSuccessStatusCode)
                {
                    string comandoDaAPI = await response.Content.ReadAsStringAsync();
                    return comandoDaAPI;
                }
                else
                {
                    Console.WriteLine($"Erro ao obter comando da API. Código de status: {response.StatusCode}");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    return null;
                }

            }
        }

        static async Task<string> NewGame(string urlNewGame)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(urlNewGame);

                if (response.IsSuccessStatusCode)
                {
                    string comandNewGame = await response.Content.ReadAsStringAsync();
                    return comandNewGame;
                }
                else
                {
                    Console.WriteLine($"Erro ao consultar novo jogo. Código de status: {response.StatusCode}");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    return null;
                }

            }
        }

        static void DeletarDadosNaAPI(string idToSend, string removeEventoBase)
        {
            string api = $"{removeEventoBase}/{idToSend}";

            using (HttpClient httpClient = new HttpClient())
            {
                var response = httpClient.DeleteAsync(api).Result;

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Dados excluídos com sucesso na API: {api}");
                }
                else
                {
                    Console.WriteLine($"Erro ao excluir dados na API: {api}. Código de status: {response.StatusCode}");
                }
            }
        }

        static void DeleteNewGame(string idNewGame, string removeNewGame)
        {
            string api = $"{removeNewGame}/{idNewGame}";

            using (HttpClient httpClient = new HttpClient())
            {
                var response = httpClient.DeleteAsync(api).Result;

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Dados excluídos com sucesso na API de New Game: {api}");
                }
                else
                {
                    Console.WriteLine($"Erro ao excluir dados na API: {api}. Código de status: {response.StatusCode}");
                }
            }
        }

        static Configuracao CarregarConfiguracoes(string arquivoConfiguracao)
        {
            string json = File.ReadAllText(arquivoConfiguracao);
            return JsonConvert.DeserializeObject<Configuracao>(json);
        }
    }
}


class RespostaConsultaMovimento
{
    public string player { get; set; }
    public string position { get; set; }
    public string piece { get; set; }
    public string inverted { get; set; }
    public string id { get; set; }
}
class respostaNewGame
{
    public string? id { get; set; }
}
class Configuracao
{
    public string? serialPortName { get; set; }
    public string? serialPortNameInverted { get; set; }
    public string? Piece { get; set; }
    public int DelaySecondsMovementQuery { get; set; }
    public int DelaySecondsToSendSerial { get; set; }
    public int DelaySecondsSendStatus { get; set; }
    public bool ActiveLoopMovementQuery { get; set; }
    public bool ActiveLoopSendStatus { get; set; }
    public bool ActiveFirstSend { get; set; }
    public bool ActiveFirstSend2 { get; set; }
    public string? UrlQueryMovement { get; set; }
    public string? UrlRemoveEvent { get; set; }
    public string? URLnewGame { get; set; }
    public string? URLnewGameOk { get; set; }
    public int baudRate { get; set; }
    public string? DropPiece { get; set; }
    public string? PushPiece { get; set; }
    public string? InitialPositionZ { get; set; }
    public string? InitialPositionX { get; set; }
    public string? InitialPositionY { get; set; }
    public string? AbsolutePosition { get; set; }
    public string? FinalPosition { get; set; }
    public string? ValueT0 { get; set; }
    public string? ValueT1 { get; set; }
    public string? ValueT2 { get; set; }
    public string? ValueT3 { get; set; }
    public string? ValueT4 { get; set; }
    public string? ValueT5 { get; set; }
    public string? ValueT6 { get; set; }
    public string? ValueT7 { get; set; }
    public string? ValueT8 { get; set; }
    public string? ValueP0 { get; set; }
    public string? ValueP1 { get; set; }
    public string? ValueP2 { get; set; }
    public string? ValueP3 { get; set; }
    public string? ValueP4 { get; set; }
    public string? StatusRobot { get; set; }
    public string? ReturnStatusRobot { get; set; }
    public string? GoDown { get; set; }
    public string? UpWithPiece { get; set; }
    public string? GoDownWithPiece { get; set; }
    public string? UpWithoutPiece { get; set; }
}
