using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1_compile
{
    internal class Lexer
    {
        private int complexFlag = 0;
        private int idFlag = 0;
        private string _input;
        private int _position;
        private List<ParseError> _errors;
        private int IDflag = 0;
        private List<Token> _tokens;

        public Lexer(string input)
        {
            _input = input;
            _position = 0;
            _errors = new List<ParseError>();
            _tokens = new List<Token>();
        }

        public List<Token> Tokenize()
        {
            _tokens.Clear();
            _errors.Clear();
            _position = 0;

            // Ожидаемые компоненты в порядке их появления
            string[] expectedComponents = {
                "Идентификатор",
                "Оператор равенства",
                "Complex",
                "Скобка открывающая",
                "Число",
                "Разделитель",
                "Число",
                "Скобка закрывающая",
                "Конец оператора"
            };

            int currentComponentIndex = 0;

            while (_position < _input.Length)
            {
                // Пропускаем пробелы
                if (char.IsWhiteSpace(_input[_position]))
                {
                    _position++;
                    continue;
                }

                // Проверяем текущий компонент
                Token token = null;
                bool isValid = false;

                switch (currentComponentIndex)
                {
                    case 0: // Идентификатор
                        token = ExtractIdentifier();
                        isValid = token != null && token.Type == "Идентификатор";
                        break;
                    case 1: // Оператор присваивания
                        if (_input[_position] == '=')
                        {
                            token = new Token(2, "Оператор", "=", _position, _position);
                            _position++;
                            isValid = true;
                        }
                        break;
                    case 2: // Ключевое слово complex
                        token = ExtractIdentifier();
                        isValid = token != null && token.Type == "Ключевое слово" && token.Value == "complex";
                        break;
                    case 3: // Открывающая скобка
                        if (_input[_position] == '(')
                        {
                            token = new Token(5, "Скобка", "(", _position, _position);
                            _position++;
                            isValid = true;
                        }
                        break;
                    case 4: // Первое число
                        token = ExtractNumber();
                        isValid = token != null && (token.Type == "Число (целое)" || token.Type == "Число (вещественное)");
                        break;
                    case 5: // Запятая
                        if (_input[_position] == ',')
                        {
                            token = new Token(4, "Разделитель", ",", _position, _position);
                            _position++;
                            isValid = true;
                        }
                        break;
                    case 6: // Второе число
                        token = ExtractNumber();
                        isValid = token != null && (token.Type == "Число (целое)" || token.Type == "Число (вещественное)");
                        break;
                    case 7: // Закрывающая скобка
                        if (_input[_position] == ')')
                        {
                            token = new Token(5, "Скобка", ")", _position, _position);
                            _position++;
                            isValid = true;
                        }
                        break;
                    case 8: // Точка с запятой
                        if (_input[_position] == ';')
                        {
                            token = new Token(6, "Конец оператора", ";", _position, _position);
                            _position++;
                            isValid = true;
                        }
                        break;
                }

                if (!isValid)
                {
                    string expected = expectedComponents[currentComponentIndex];
                    string actual = token?.Type ?? "неизвестный символ";
                    _errors.Add(new ParseError($"Ошибка: ожидался {expected}",
                        new Token(-1, "Ошибка", "", _position, _position)));
                    return _tokens; // Возвращаем токены до ошибки
                }

                _tokens.Add(token);
                currentComponentIndex++;

                // Если достигли конца структуры, проверяем наличие лишних символов
                if (currentComponentIndex == expectedComponents.Length)
                {
                    while (_position < _input.Length)
                    {
                        if (!char.IsWhiteSpace(_input[_position]))
                        {
                            _errors.Add(new ParseError("Ошибка: обнаружены лишние символы после точки с запятой",
                                new Token(-1, "Ошибка", "", _position, _position)));
                            return _tokens;
                        }
                        _position++;
                    }
                }
            }

            // Если не достигли конца структуры
            if (currentComponentIndex < expectedComponents.Length)
            {
                _errors.Add(new ParseError($"Ошибка: неполная структура. Ожидался {expectedComponents[currentComponentIndex]}",
                    new Token(-1, "Ошибка", "", _position, _position)));
            }

            return _tokens;
        }

        private void CheckComplexNumberStructure()
        {
            // Проверяем наличие всех необходимых компонентов
            bool hasIdentifier = false;
            bool hasAssignment = false;
            bool hasComplex = false;
            bool hasOpenBracket = false;
            bool hasFirstNumber = false;
            bool hasComma = false;
            bool hasSecondNumber = false;
            bool hasCloseBracket = false;
            bool hasSemicolon = false;

            // Проверяем компоненты по порядку
            foreach (var token in _tokens)
            {
                if (token == null) continue;

                if (!hasIdentifier)
                {
                    if (token.Type == "Идентификатор")
                    {
                        hasIdentifier = true;
                        continue;
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ошибка: ожидался идентификатор",
                            new Token(-1, "Ошибка", "", 0, 0)));
                        break;
                    }
                }

                if (!hasAssignment)
                {
                    if (token.Type == "Оператор" && token.Value == "=")
                    {
                        hasAssignment = true;
                        continue;
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ошибка: ожидался оператор присваивания (=)",
                            new Token(-1, "Ошибка", "", 0, 0)));
                        break;
                    }
                }

                if (!hasComplex)
                {
                    if (token.Type == "Ключевое слово" && token.Value == "complex")
                    {
                        hasComplex = true;
                        continue;
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ошибка: ожидалось ключевое слово 'complex'",
                            new Token(-1, "Ошибка", "", 0, 0)));
                        break;
                    }
                }

                if (!hasOpenBracket)
                {
                    if (token.Type == "Скобка" && token.Value == "(")
                    {
                        hasOpenBracket = true;
                        continue;
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ошибка: ожидалась открывающая скобка (",
                            new Token(-1, "Ошибка", "", 0, 0)));
                        break;
                    }
                }

                if (!hasFirstNumber)
                {
                    if (token.Type == "Число (целое)" || token.Type == "Число (вещественное)")
                    {
                        hasFirstNumber = true;
                        continue;
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ошибка: ожидалось первое число",
                            new Token(-1, "Ошибка", "", 0, 0)));
                        break;
                    }
                }

                if (!hasComma)
                {
                    if (token.Type == "Разделитель" && token.Value == ",")
                    {
                        hasComma = true;
                        continue;
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ошибка: ожидалась запятая",
                            new Token(-1, "Ошибка", "", 0, 0)));
                        break;
                    }
                }

                if (!hasSecondNumber)
                {
                    if (token.Type == "Число (целое)" || token.Type == "Число (вещественное)")
                    {
                        hasSecondNumber = true;
                        continue;
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ошибка: ожидалось второе число",
                            new Token(-1, "Ошибка", "", 0, 0)));
                        break;
                    }
                }

                if (!hasCloseBracket)
                {
                    if (token.Type == "Скобка" && token.Value == ")")
                    {
                        hasCloseBracket = true;
                        continue;
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ошибка: ожидалась закрывающая скобка )",
                            new Token(-1, "Ошибка", "", 0, 0)));
                        break;
                    }
                }

                if (!hasSemicolon)
                {
                    if (token.Type == "Конец оператора" && token.Value == ";")
                    {
                        hasSemicolon = true;
                        continue;
                    }
                    else
                    {
                        _errors.Add(new ParseError("Ошибка: ожидалась точка с запятой",
                            new Token(-1, "Ошибка", "", 0, 0)));
                        break;
                    }
                }
            }

            // Если все компоненты на месте, но есть лишние токены
            if (hasSemicolon && _tokens.Count > 9)
            {
                _errors.Add(new ParseError("Ошибка: обнаружены лишние символы после точки с запятой",
                    new Token(-1, "Ошибка", "", 0, 0)));
            }
        }

        private Token ExtractIdentifier()
        {
            if (complexFlag == 1)
            {
                _position++;
                return null;
            }
            int start = _position;
            while (_position < _input.Length && (char.IsLetterOrDigit(_input[_position])))
            {
                _position++;
            }

            string value = _input.Substring(start, _position - start);
            if (value == "complex")
            {
                return new Token(13, "Ключевое слово", value, start, _position - 1);
            }
            else if (_position < 5)
            {
                if (idFlag == 1)
                {
                    while (_input[_position + 1] != '=')
                    {
                        _position++;
                    }
                    return null;
                }
                    idFlag = 1;
                return new Token(1, "Идентификатор", value, start, _position - 1);
            }
            else
            {
                if (complexFlag == 0)
                {
                    //_errors.Add(new ParseError($"Неверное написание ключевого слова complex",
                    //    new Token(-2, "Ошибка", value, start, _position - 1)));
                    while (_input[_position] == '@' || (char.IsLetter(_input[_position])))
                    {
                        _position++;
                    };
                    complexFlag = 1;
                    
                }
                return null;
                //return new Token(1, "Идентификатор", value, start, _position - 1);
            }

            //return new Token(1, "Идентификатор", value, start, _position - 1);
        }

        

            //return new Token(1, "Идентификатор", value, start, _position - 1);
        private Token ExtractNumber()
        {
            int start = _position;
            bool hasDecimal = false;
            complexFlag = 2;
            if (_input[_position] == '-')
            {
                _position++;
            }

            while (_position < _input.Length && char.IsDigit(_input[_position]))
            {
                _position++;
            }

            if (_position < _input.Length && _input[_position] == '.')
            {
                hasDecimal = true;
                int dotPosition = _position;
                _position++;

                if (_position >= _input.Length || !char.IsDigit(_input[_position]))
                {
                    _errors.Add(new ParseError($"Ошибка: ожидалось число после точки, найдено '{_input.Substring(start, _position - start)}'",
                        new Token(-1, "Ошибка", _input.Substring(start, _position - start), start, _position - 1)));

                    _position++;
                    return null;
                }

                while (_position < _input.Length && char.IsDigit(_input[_position]))
                {
                    _position++;
                }
            }

            string value = _input.Substring(start, _position - start);
            return new Token(hasDecimal ? 9 : 8, hasDecimal ? "Число (вещественное)" : "Число (целое)", value, start, _position - 1);
        }

        public List<ParseError> GetErrors()
        {
            return _errors;
        }
    }
}