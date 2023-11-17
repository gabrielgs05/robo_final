using System;
using System.IO;
using System.IO.Ports;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

class Program {
    static async Task Main(string[] args) {
        Configuracao configuracao = CarregarConfiguracoes(@"C:\Users\gabriel.sousa_siteme\Desktop\robo_final\config.json");

        async Task EsperarAteLiberado(SerialPort port, string liberado, TimeSpan delay) {
            while (true) {
                port.WriteLine(configuracao.StatusRobo);
                Console.WriteLine("Enviando comando para verificar status...");
                if (port.ReadLine().Contains(liberado)) { 
                    Console.WriteLine("Liberado!");
                break;
                }
                await Task.Delay(delay);
            }
        }

        Dictionary<string, string> positionMapping = new Dictionary<string, string> {
        { "T0", configuracao.ValorT0 },
        { "T1", configuracao.ValorT1 },
        { "T2", configuracao.ValorT2 },
        { "T3", configuracao.ValorT3 },
        { "T4", configuracao.ValorT4 },
        { "T5", configuracao.ValorT5 },
        { "T6", configuracao.ValorT6 },
        { "T7", configuracao.ValorT7 },
        { "T8", configuracao.ValorT8 },
        { "P0", configuracao.ValorP0 },
        { "P1", configuracao.ValorP1 },
        { "P2", configuracao.ValorP2 },
        { "P3", configuracao.ValorP3 },
        { "P4", configuracao.ValorP4 },
        { "DropPiece", configuracao.DropPiece },
        { "PushPiece", configuracao.PushPiece },
        { "InitialPositionZ", configuracao.InitialPositionZ },
        { "InitialPositionX", configuracao.InitialPositionX },
        { "InitialPositionY", configuracao.InitialPositionY },
        { "AbsolutePosition", configuracao.AbsolutePosition },
        { "FinalPosition", configuracao.FinalPosition },
        { "DescePeca", configuracao.DescePeca },
        { "SobePeca", configuracao.SobePeca },
        { "DescePecaTabuleiro", configuracao.DescePecaTabuleiro },
        { "SobeSemPecaTabuleiro", configuracao.SobeSemPecaTabuleiro },};

        Dictionary<string, string> positionInvertedMapping = new Dictionary<string, string> {
        { "T0", configuracao.ValorT8 },
        { "T1", configuracao.ValorT7 },
        { "T2", configuracao.ValorT6 },
        { "T3", configuracao.ValorT5 },
        { "T4", configuracao.ValorT4 },
        { "T5", configuracao.ValorT3 },
        { "T6", configuracao.ValorT2 },
        { "T7", configuracao.ValorT1 },
        { "T8", configuracao.ValorT0 },
        { "P0", configuracao.ValorP0 },
        { "P1", configuracao.ValorP1 },
        { "P2", configuracao.ValorP2 },
        { "P3", configuracao.ValorP3 },
        { "P4", configuracao.ValorP4 },
        { "DropPiece", configuracao.DropPiece },
        { "PushPiece", configuracao.PushPiece },
        { "InitialPositionZ", configuracao.InitialPositionZ },
        { "InitialPositionX", configuracao.InitialPositionX },
        { "InitialPositionY", configuracao.InitialPositionY },
        { "AbsolutePosition", configuracao.AbsolutePosition },
        { "FinalPosition", configuracao.FinalPosition },
        { "DescePeca", configuracao.DescePeca },
        { "SobePeca", configuracao.SobePeca },
        { "DescePecaTabuleiro", configuracao.DescePecaTabuleiro },
        { "SobeSemPecaTabuleiro", configuracao.SobeSemPecaTabuleiro },};

        while (configuracao.AtivarLoopConsultaMovimento) {
            string urlConsultaMovimento = configuracao.URLconsultaMovimentoBase; //+ configuracao.Piece;
            string comandoDaAPI = await LerComandoDaAPI(urlConsultaMovimento);

            if (comandoDaAPI != null) {
                var listaResultado = JsonConvert.DeserializeObject<List<RespostaConsultaMovimento>>(comandoDaAPI);
                //var resultadoConsultaMovimento = JsonConvert.DeserializeObject<RespostaConsultaMovimento>(comandoDaAPI);
                foreach (var resultadoConsultaMovimento in listaResultado) {
                string player = resultadoConsultaMovimento.player;
                string pieceToSend = resultadoConsultaMovimento.piece;
                string idToSend = resultadoConsultaMovimento.id;
                string positionToSend = resultadoConsultaMovimento.position;
                string inverted = resultadoConsultaMovimento.inverted;
                string liberado = configuracao.RetornoStatusRobo;
                string initialPositionZ = "InitialPositionZ";
                string initialPositionX = "InitialPositionX";
                string initialPositionY = "InitialPositionY";
                string AbsolutePosition = "AbsolutePosition";
                string descePeca = "DescePeca";
                string sobePeca = "SobePeca";
                string descePecaTabuleiro = "DescePecaTabuleiro";
                string dropPiece = "DropPiece";
                string pushPiece = "PushPiece";
                string sobeSemPecaTabuleiro = "SobeSemPecaTabuleiro";
                string finalPosition = "FinalPosition";

                if (!string.IsNullOrEmpty(pieceToSend) && !string.IsNullOrEmpty(positionToSend)) {
                    using (SerialPort serialPort = new SerialPort(inverted == "true" ? configuracao.serialPortNameInverted : configuracao.serialPortName)) {
                        serialPort.BaudRate = 250000;
                        serialPort.Encoding = Encoding.UTF8;
                        serialPort.NewLine = "\n";
                        serialPort.Open();

                        //string[] positions = inverted == "true" ? positionInvertedMapping.Keys.ToArray() : positionMapping.Keys.ToArray();

                        foreach (string command in new string[] { initialPositionZ, initialPositionX, initialPositionY, AbsolutePosition, finalPosition, pieceToSend, descePeca, pushPiece, sobePeca, positionToSend, descePecaTabuleiro, dropPiece, sobeSemPecaTabuleiro, finalPosition }) {
                            string selectedPosition = inverted == "true" ?
                                (positionInvertedMapping.ContainsKey(command) ? positionInvertedMapping[command] : throw new Exception($"Comando não mapeado: {command}")) :
                                (positionMapping.ContainsKey(command) ? positionMapping[command] : throw new Exception($"Comando não mapeado: {command}"));

                            serialPort.WriteLine(selectedPosition);
                            await EsperarAteLiberado(serialPort, liberado, TimeSpan.FromSeconds(configuracao.DelaySecondsToSendSerial));
                            Console.WriteLine($"Comando executado com sucesso: {command}");
                        }

                        DeletarDadosNaAPI(idToSend, configuracao.URLremoveEventoBase);
                    }
                }
            }
            }

            await Task.Delay(TimeSpan.FromSeconds(configuracao.DelaySecondsConsultaMovimento));
        }
    }

    static async Task<string> LerComandoDaAPI(string urlConsultaMovimento) {
        using (HttpClient httpClient = new HttpClient()) {
            var response = await httpClient.GetAsync(urlConsultaMovimento);

            if (response.IsSuccessStatusCode) {
                string comandoDaAPI = await response.Content.ReadAsStringAsync();
                return comandoDaAPI;
            }
            else {
                Console.WriteLine($"Erro ao obter comando da API. Código de status: {response.StatusCode}");
                return null;
            }
        }
    }

    static void DeletarDadosNaAPI(string idToSend, string removeEventoBase) {
        string api = $"{removeEventoBase}/{idToSend}";

        using (HttpClient httpClient = new HttpClient()) {
            var response = httpClient.DeleteAsync(api).Result;

            if (response.IsSuccessStatusCode) {
                Console.WriteLine($"Dados excluídos com sucesso na API: {api}");
            }
            else {
                Console.WriteLine($"Erro ao excluir dados na API: {api}. Código de status: {response.StatusCode}");
            }
        }
    }

    static Configuracao CarregarConfiguracoes(string arquivoConfiguracao) {
        string json = File.ReadAllText(arquivoConfiguracao);
        return JsonConvert.DeserializeObject<Configuracao>(json);
    }
}


class RespostaConsultaMovimento {
    public string player { get; set; }
    public string position { get; set; }
    public string piece { get; set; }
    public string inverted { get; set; }
    public string id { get; set; }
}

class Configuracao {
    public string? serialPortName { get; set; }
    public string? serialPortNameInverted { get; set; }
    public string? Piece { get; set; }
    public int DelaySecondsConsultaMovimento { get; set; }
    public int DelaySecondsToSendSerial { get; set; }
    public int DelaySecondsEnviaStatus { get; set; }
    public bool AtivarLoopConsultaMovimento { get; set; }
    public bool AtivarLoopEnviaStatus { get; set; }
    public string? URLconsultaMovimentoBase { get; set; }
    public string? URLremoveEventoBase { get; set; }
    public string? DropPiece { get; set; }
    public string? PushPiece { get; set; }
    public string? InitialPositionZ { get; set; }
    public string? InitialPositionX { get; set; }
    public string? InitialPositionY { get; set; }
    public string? AbsolutePosition { get; set; }
    public string? FinalPosition { get; set; }
    public string? ValorT0 { get; set; }
    public string? ValorT1 { get; set; }
    public string? ValorT2 { get; set; }
    public string? ValorT3 { get; set; }
    public string? ValorT4 { get; set; }
    public string? ValorT5 { get; set; }
    public string? ValorT6 { get; set; }
    public string? ValorT7 { get; set; }
    public string? ValorT8 { get; set; }
    public string? ValorP0 { get; set; }
    public string? ValorP1 { get; set; }
    public string? ValorP2 { get; set; }
    public string? ValorP3 { get; set; }
    public string? ValorP4 { get; set; }
    public string? StatusRobo { get; set; }
    public string? RetornoStatusRobo { get; set; }
    public string? DescePeca { get; set; }
    public string? SobePeca { get; set; }
    public string? DescePecaTabuleiro { get; set; }
    public string? SobeSemPecaTabuleiro { get; set; }
}

