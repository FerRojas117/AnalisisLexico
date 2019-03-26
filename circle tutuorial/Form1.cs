using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace circle_tutuorial
{

    public partial class Form1 : Form
    {
        /*
         * listas para almacenar 
         * 
         */
        List<Caracteres> caracteres = new List<Caracteres>();
        List<string> reservadas = new List<string>();
        List<Tokens> tokens = new List<Tokens>();
        List<EstadoAssignToken> eat = new List<EstadoAssignToken>();
        List<string> estados = new List<string>();
        List<int> numEstados = new List<int>();
        List<Tokencillo> tokencillo = new List<Tokencillo>();


        List<string> alfabeto = new List<string>();
        List<int> alfabetoNumero = new List<int>();


        List<Transiciones> transiciones = new List<Transiciones>();

        List<Finales> finales = new List<Finales>();
        List<Lineas> lineas = new List<Lineas>();
        List<TokensEnArchivo> tea = new List<TokensEnArchivo>();

        ClaseMatriz cm;

        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {

            // Empieza la lectura de archivo, del lenguaje a utilizar
            var fileContent = string.Empty;
            var filePath = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;
                //Read the contents of the file into a stream
                var fileStream = openFileDialog.OpenFile();
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    fileContent = reader.ReadToEnd();
                }

            }
            // Termina la lectura de archivo, del lenguaje a utilizar


            /*
             * Empieza el almacenamiento del archivo, la idea aquí es
             * leer el lenguaje del archivo y almacenarlo en 
             * las clases de caracteres y tokens ya construidas 
             */

            string tokens = fileContent;
            string[] tokens2 = tokens.Split(' ', '\r', '\0', '\n');

            String[] datosnew = new String[tokens2.Length];
            int j = 0;
            for (int i = 0; i < tokens2.Length; i++)
            {

                if (tokens2[i] != "")
                {
                    datosnew[j] = tokens2[i];
                    j++;
                }
            }
            int contador = datosnew.Length;
            int placeForTokens = 0;
            // EMPIEZAN A LLENARSE LOS OBJETOS
            for (int x = 0; x < contador; x++)
            {
                if (datosnew[x] == "CARACTERES")
                {
                    x++;
                    while (datosnew[x] != "TOKENS")
                    {
                        Caracteres c = new Caracteres(datosnew[x], datosnew[x + 2]);
                        caracteres.Add(c);
                        x += 3;
                        Console.WriteLine("C: " + datosnew[x]);
                    }
                    placeForTokens = x + 1;
                    break;
                }
            }

            int placeValueTokens = placeForTokens + 2;
            for (int i = placeForTokens; i < contador; i++)
            {
                while (datosnew[i] != "FINDEGRAMATICA" && datosnew[i] != "RESERVADAS")
                {
                    List<string> valDeToks = new List<string>();
                    placeValueTokens = i + 2;
                    while (datosnew[placeValueTokens] != ",")
                    {
                        Console.WriteLine("ADDED: " + datosnew[placeValueTokens]);
                        valDeToks.Add(datosnew[placeValueTokens]);
                        placeValueTokens++;
                    }
                    Tokens t = new Tokens(datosnew[i], valDeToks);
                    this.tokens.Add(t);
                    i = placeValueTokens + 1;
                    Console.WriteLine(datosnew[i]);
                }
                break;
            }

            for (int i = 0; i < contador; i++)
            {
                if (datosnew[i] == "RESERVADAS")
                {
                    i++;
                    while (datosnew[i] != "FINDEGRAMATICA")
                    {
                        string res = datosnew[i];
                        reservadas.Add(res);
                        i++;
                    }
                    break;
                }
            }
            

            foreach (string s in reservadas)
            {
                Console.WriteLine("reservada: " + s);
            }

            foreach (Caracteres c in caracteres)
            {
                Console.WriteLine("Variable: " + c.variable + " Caracter: " + c.valor);
            }
            foreach (Tokens t in this.tokens)
            {
                Console.Write("Variable: " + t.variable + " ");
                foreach (string s in t.valoresDeToken)
                {
                    Console.Write("Token: " + s + " ");
                }
                Console.WriteLine();
            }

            Caracteres otro = new Caracteres("otro", "otro");
            Caracteres espacio = new Caracteres(" ", " ");
            Caracteres saltoLinea = new Caracteres("\n", "\n");
            Caracteres tabulador = new Caracteres("\t", "\t");

            caracteres.Add(otro);
            caracteres.Add(espacio);
            caracteres.Add(saltoLinea);
            caracteres.Add(tabulador);
            //linea añadida


            // se crea matriz con un objeto de la ClaseMatriz
            // la clase matriz debe ser modificada para albergar los campos que sean
            // necesarios, al menos debe tener el de Token, para saber qué tipo 
            // de token sigió el autómata
            ContarEstadosYhacerTrans();

            foreach (Transiciones t in transiciones)
            {
                Console.WriteLine("EI: " + t.estadoInicial + " Tok: " + t.caracter + " EF: " + t.estadoFinal);
            }


            cm = new ClaseMatriz(numEstados.Count, caracteres.Count);
            // Se deben cambiar posiciones en matriz, modificar funciones de getposEstados
            // y get posicion alfabeto para obtener las posiciones y en efecto, cambiar
            // esa posición con el estado al que conduce esa posicion
            int col = 0;

            foreach (Transiciones s in transiciones)
            {
                col = getPosAlfa(s.caracter);
                cm.CambiarPosicion(Convert.ToInt32(s.estadoInicial), col, Convert.ToInt32(s.estadoFinal));
            }

            cm.ImprimirMatriz();
        }

        // metodo que se tiene que modificar, para obtener la posicion del caracter
        // en otras palabras, de la columna de la matriz correspondiente
        public int getPosAlfa(string s)
        {

            int pos = 0;
            foreach (Caracteres car in caracteres)
            {
                if (s == car.valor) return pos;
                pos++;
            }
            return pos;
        }



        public void ContarEstadosYhacerTrans()
        {
            int estados = 0;
            int estadoInicial = 0;
            int estadoActual = 0;
            string repeticion = "*";
            bool flagHasRepetition = false;
            bool flagForRepeated = false;

            numEstados.Add(estados);

            foreach (Tokens t in tokens)
            {
                int tamValTokens = t.valoresDeToken.Count;
                flagHasRepetition = false;
                flagForRepeated = false;
                // valores del token 
                for (int i = 0; i < tamValTokens; i++)
                {
                    string valToken = t.valoresDeToken[i];
                    //Console.WriteLine("TOKEN a tratar: " + valToken );

                    flagHasRepetition = false;
                    if (valToken.Contains(repeticion))
                    {
                        valToken = t.valoresDeToken[i].Trim(new Char[] { '*' });
                        flagHasRepetition = true;
                    }
                    // si hubo un caracter repetido en token, saltar hasta la posicion del path 
                    foreach (Caracteres c in caracteres)
                    {
                        flagForRepeated = false;
                        if (valToken == c.variable)
                        {
                            foreach (Transiciones tr in transiciones)
                            {
                                foreach (int x in numEstados)
                                {
                                    if (tr.estadoInicial == estadoActual && tr.caracter == c.valor && tr.estadoFinal == x)
                                    {
                                        estadoActual = tr.estadoFinal;
                                        flagForRepeated = true;
                                        break;
                                    }
                                }

                            }
                            if (flagForRepeated) { break; }

                            if (flagHasRepetition)
                            {
                                Transiciones transiConRepeticion = new Transiciones(estadoActual, c.valor, estadoActual);
                                Console.WriteLine("EA: " + estadoActual + " TOK: " + c.valor + " EF: " + estadoActual);
                                transiciones.Add(transiConRepeticion);
                                flagHasRepetition = false;
                            }
                            else
                            {
                                Console.WriteLine("Sin rep");
                                Transiciones transiSinRepeticion = new Transiciones(estadoActual, c.valor, numEstados.Count);
                                Console.WriteLine("EA: " + estadoActual + " TOK: " + c.valor + " EF: " + numEstados.Count);
                                transiciones.Add(transiSinRepeticion);
                                estadoActual = numEstados.Count;
                                estados++;
                                numEstados.Add(estados);
                                Console.WriteLine("ESTADO AGREGADO: " + estados);
                            }
                        }
                    }
                }
                // agregar estado ultimo, de transicion de otro.
                //linea añadida
                Transiciones transi = new Transiciones(estadoActual, "otro", estadoActual + 1);
                Console.WriteLine("EA: " + estadoActual + " TOK: otro" + " EF: " + (estadoActual + 1));
                //linea añadida
                transiciones.Add(transi);
                estados++;
                numEstados.Add(estados);

                Transiciones caracterBlank = new Transiciones(estadoActual, " ", estadoActual + 2);
                Console.WriteLine("EA: " + estadoActual + " TOK: espacio" + " EF: " + (estadoActual + 2));
                transiciones.Add(caracterBlank);
                estados++;
                numEstados.Add(estados);

                Transiciones caracterEspacio = new Transiciones(estadoActual, "\n", estadoActual + 3);
               // Console.WriteLine("EA: " + estadoActual + " TOK: espacio" + " EF: " + (estadoActual + 2));
                transiciones.Add(caracterEspacio);
                estados++;
                numEstados.Add(estados);

                Transiciones caracterTabulador = new Transiciones(estadoActual, "\t", estadoActual + 4);
                //Console.WriteLine("EA: " + estadoActual + " TOK: espacio" + " EF: " + (estadoActual + 2));
                transiciones.Add(caracterTabulador);
                estados++;
                numEstados.Add(estados);


                Finales f = new Finales(estadoActual + 1, t.variable);
                Finales f_blank = new Finales(estadoActual + 2, t.variable);
                Finales f_espacio = new Finales(estadoActual + 3, t.variable);
                Finales f_tabulador = new Finales(estadoActual + 4, t.variable);

                finales.Add(f);
                finales.Add(f_blank);
                finales.Add(f_espacio);
                finales.Add(f_tabulador);
                estadoActual = estadoInicial;
            }
        }


        /*
         * metodo que se acciona cuando se presiona el boton de checar cadena
         * basicamente obtiene el texto en la caja 2 y lo trata con 
         * el automata para determinar si la cadena es correcta
         */
        private void button1_Click(object sender, EventArgs e)
        {
            int contadorLineas = textBox1.Lines.Count();
            bool isNumber;
            bool isLetter;
            string errDeCaracter = "";
            int posicion = 0;
            int k = 0;
            List<string> expectedChars = new List<string>();
            TokensEnArchivo tar;
   
            for (int i = 0; i < contadorLineas; i++)
            {
                for (int j = 0; j < textBox1.Lines[i].Length; j++)
                {
                    string caracter = textBox1.Lines[i][j].ToString();
                    if (String.IsNullOrEmpty(caracter)) continue;
                    if (caracter == "\n") caracter = " ";
                    if (caracter == "\t") caracter = " ";
                    isNumber = checkIfNumero(caracter);
                    isLetter = checkIfLetter(caracter);
                    Lineas l = new Lineas(i + 1, caracter, isNumber, isLetter);
                    lineas.Add(l);
                }
                k = i + 1;
            }
            
            foreach (Caracteres c in caracteres)
            {
                if (lineas[lineas.Count - 1].caracter != c.valor && c.valor != "NUMERO" && c.valor != "ALFABETO" && c.valor != " ")
                {
                    Lineas final = new Lineas(k, c.valor, false, false);
                    Console.WriteLine("VALOR FINAL: " + c.valor);
                    lineas.Add(final);
                    break;
                }
            }

            while (posicion < lineas.Count)
            {
                Console.WriteLine("Esta en ciclo.");
                tar = AnalisisLexico(ref posicion, ref errDeCaracter);
                if(tar.valor == "ERROR400")
                {
                    textBox2.AppendText(errDeCaracter);
                    break;
                }
                else if (tar.valor == "EOF")
                { 
                    break;
                }
                tea.Add(tar);
                Console.WriteLine("Linea: " + tar.numeroLinea + " Variable: " + tar.variable + " TOKEN: " + tar.valor);
            }

            foreach (TokensEnArchivo t in tea)
            {
               textBox2.AppendText("<Linea: " + t.numeroLinea + " Variable: \"" + t.variable + "\" TOKEN: \"" + t.valor + "\">\n");
            }
                                    
            textBox2.AppendText("--------------------------------------------------------------" + "\n");
            tea.Clear();
            lineas.Clear();
            expectedChars.Clear();
        }

      
        public TokensEnArchivo AnalisisLexico(ref int pos, ref string errDeCaracter)
        {
            int estadoActual = 0;
            int valor;

            bool isExpected = false;

            string tok = "";
            string tipo = "";
            TokensEnArchivo tokADevolver;
            List<string> expectedChars = new List<string>();

            while(lineas[pos].caracter == " " || lineas[pos].caracter == "\n" || lineas[pos].caracter == "\t")
            {
                pos++;
            }

            for (; pos < lineas.Count; pos++)
            {
                isExpected = false;
                foreach (Finales f in finales)
                {
                    if (estadoActual == f.estado)
                    {
                        tok = tok.Trim(new Char[] { ' ', '\n', '\t' });
                        foreach (string reserv in reservadas)
                        {
                            if (tok == reserv)
                            {
                                Console.WriteLine("ENTRO A RESERV" + " Variable: " + tok + " TOKEN: " + tok);
                                tokADevolver = new TokensEnArchivo(lineas[pos].numeroLinea, reserv, tok);
                                return tokADevolver;
                            }
                        }
                        tokADevolver = new TokensEnArchivo(lineas[pos].numeroLinea, f.token, tok);
                        return tokADevolver;  
                    }
                }
                expectedChars = expectedChar(estadoActual);

                foreach (string s in expectedChars)
                {
                    if (lineas[pos].isNumber) tipo = "NUMERO";
                    else if (lineas[pos].isLetter) tipo = "ALFABETO";
                    else tipo = lineas[pos].caracter;
                    if (tipo == s)
                    {
                        valor = getPosAlfa(tipo);
                        estadoActual = cm.Matriz[estadoActual, valor];
                        tok += lineas[pos].caracter;
                        isExpected = true;
                        break;
                    }
                }
                if (!isExpected)
                {
                    tipo = "otro";
                    Console.WriteLine("ES OTRO");
                    valor = getPosAlfa(tipo);
                    if (cm.Matriz[estadoActual, valor] == -1)
                    {
                        
                        errDeCaracter= "TOKEN INESPERADO: " + lineas[pos].caracter + ". TOKEN ESPERADO: ";
                        tok = "ERROR400";
                        foreach (string s in expectedChars)
                        {
                            errDeCaracter += s + " || ";
                        }
                        textBox2.AppendText("\n");
                        lineas.Clear();
                        expectedChars.Clear();
                        tea.Clear();
                        tokADevolver = new TokensEnArchivo(1, tok, tok);
                        return tokADevolver;
                    }
                    estadoActual = cm.Matriz[estadoActual, valor];
                    pos--;
                }
            }
            tokADevolver = new TokensEnArchivo(lineas[pos-1].numeroLinea, "EOF", "EOF");
            return tokADevolver;
        }


        private void button3_Click(object sender, EventArgs e)
        {
            caracteres.Clear();
            tokens.Clear();
            eat.Clear();
            estados.Clear();
            numEstados.Clear();
            tokencillo.Clear();


            alfabeto.Clear();
            alfabetoNumero.Clear();
            transiciones.Clear();

            finales.Clear();
            lineas.Clear();
            tea.Clear();
            cm = null;
        }

        public bool checkIfNumero(string supuestoNumero)
        {
            int number1 = 0;
            if (int.TryParse(supuestoNumero, out number1)) return true;
            return false;
        }

        public bool checkIfLetter(string supuestaLetra)
        {
            char check = supuestaLetra[0];
            if (Char.IsLetter(check)) return true;
            return false;
        }

        public List<string> expectedChar(int estadoActual)
        {
            // se puede hacer una estrructura de datos para obtener los token esperados en la siguoiente iteracion
            List<string> expectedChars = new List<string>();
            foreach (Transiciones t in transiciones)
            {
                if (t.estadoInicial == estadoActual)
                {
                    Console.WriteLine("Expected: " + t.caracter);
                    expectedChars.Add(t.caracter);
                }
            }
            return expectedChars;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                //Get the path of specified file
                filePath = openFileDialog.FileName;
                //Read the contents of the file into a stream
                var fileStream = openFileDialog.OpenFile();
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    fileContent = reader.ReadToEnd();
                }
            }
            string contenido = fileContent;
            textBox1.AppendText(contenido);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }

    // clase matriz, que se tiene que modificar, ver instruccion en linea 187
    public class ClaseMatriz
    {
        public int[,] Matriz;
        public int tamRenglones, tamColumnas;

        public ClaseMatriz(int x, int y)
        {
            tamRenglones = x;
            tamColumnas = y;
            Matriz = new int[tamRenglones, tamColumnas];
            for (int i = 0; i < tamRenglones; i++)
            {
                for (int j = 0; j < tamColumnas; j++)
                {
                    Matriz[i, j] = -1;
                }
            }
        }

        public void CambiarPosicion(int x, int y, int valor)
        {
            Matriz[x, y] = valor;
        }

        public void ImprimirMatriz()
        {
            for (int i = 0; i < tamRenglones; i++)
            {
                for (int j = 0; j < tamColumnas; j++)
                {
                    Console.Write("{0} ", Matriz[i, j]);
                }
                Console.WriteLine();
            }
        }
    }

    // clase caracteres, para almacenar en una lista e ir almacenando objetos de esta clase 
    class Caracteres
    {
        public string variable, valor;
        public Caracteres(string variable, string valor)
        {
            this.variable = variable;
            this.valor = valor;
        }
    }

    // clase Tokens, para almacenar en una lista e ir almacenando objetos de esta clase 
    class Transiciones
    {
        public int estadoInicial;
        public string caracter;
        public int estadoFinal;

        public Transiciones(int estadoInicial, string caracter, int estadoFinal)
        {
            this.estadoInicial = estadoInicial;
            this.estadoFinal = estadoFinal;
            this.caracter = caracter;
        }
    }

    // clase Tokens, para almacenar en una lista e ir almacenando objetos de esta clase 
    class Tokens
    {
        public string variable;
        public List<string> valoresDeToken;

        public Tokens(string variable, List<string> valoresDeToken)
        {
            this.variable = variable;
            this.valoresDeToken = valoresDeToken;
        }
    }

    // clase Tokens, para almacenar en una lista e ir almacenando objetos de esta clase 
    class EstadoAssignToken
    {
        public string caracter;
        public int estado;
        public EstadoAssignToken(string caracter, int estado)
        {
            this.caracter = caracter;
            this.estado = estado;
        }
    }

    class Finales
    {
        public int estado;
        public string token;

        public Finales(int estado, string token)
        {
            this.estado = estado;
            this.token = token;
        }
    }

    // clase Tokens, para almacenar en una lista e ir almacenando objetos de esta clase 
    class Tokencillo
    {
        public string token;
        public string valor;
        public int noLinea;

        public Tokencillo(string token, string valor, int noLinea)
        {
            this.token = token;
            this.valor = valor;
            this.noLinea = noLinea;
        }
    }

    class Lineas
    {
        public int numeroLinea;
        public string caracter;
        public bool isNumber;
        public bool isLetter;

        public Lineas(int numeroLinea, string caracter, bool isNumber, bool isLetter)
        {
            this.numeroLinea = numeroLinea;
            this.caracter = caracter;
            this.isNumber = isNumber;
            this.isLetter = isLetter;
        }
    }

   public class TokensEnArchivo
    {
        public int numeroLinea;
        public string variable;
        public string valor;

        public TokensEnArchivo(int numeroLinea, string variable, string valor)
        {
            this.numeroLinea = numeroLinea;
            this.variable = variable;
            this.valor = valor;
        }
    }
}