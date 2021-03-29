using System.Collections.Generic;

namespace MasterCsharpHosted.Shared
{
    public static class CodeSnippets
    {
        public static Dictionary<string, Dictionary<string, string>> AllSnippets()
        {
            return new()
            {
                {"Collections", CollectionsSnippets},
                {"Strings", StringsSnippets},
                {"Conditionals", ConditionalSnippets},
                {"Extension methods", ExtensionSnippets}
            };
        }

        #region Linq from Github

        public static readonly CodeSamples LinqFromGithubSamples = new()
        {
            Samples = new List<CodeSample>
            {
                new("Linq Filter", "LinqFilter", "<p>The Where operator (Linq extension method) filters the collection based on a given criteria expression and returns a new collection. The criteria can be specified as lambda expression or Func delegate type.The Where extension method has two overloads. Both overload methods accepts a Func delegate type parameter. One overload required Func<TSource,bool> input parameter and second overload method required Func<TSource, int, bool> input parameter where int is for index</p><p>The Select() operator always returns an IEnumerable collection which contains elements based on a transformation function. It is similar to the Select clause of SQL that produces a flat result set.</p>", "There are a variety of ways to filter and sort your collections. .Where(), First(), using Select() are the most common"),
                new("Linq Ordering", "LinqOrderBy", "<p>.OrderBy() Sorts the elements in the collection based on specified fields in ascending or decending order.</p><p>.OrderByDescending() Sorts the collection based on specified fields in descending order. Only valid in method syntax.</p>", "Reorder your collection with OrderBy() and OrderByDescending()"),
                new( "Linq Aggregate", "LinqAggregate", "<p>The aggregation operators perform mathematical operations like Average, Aggregate, Count, Max, Min and Sum, on the numeric property of the elements in the collection.</p><p>.Aggregate Performs a custom aggregation operation on the values in the collection.</p>", "Aggregate() performs a specific or custom aggregation operation"),
                new("Linq with Numbers", "LinqMaths", "<p>.Average() calculates the average of the numeric items in the collection.</p><p>.Count Counts the elements in a collection.</p><p>.Max() Finds the largest value in the collection.</p><p>.Sum Calculates sum of the values in the collection.</p>", "Do a some simple math operations with .Average(), .Sum(), .Max(), .Min()"),
                new("Distinct and Except", "LinqSet1", "<p>.Distinct() The Distinct extension method returns a new collection of unique elements from the given collection.</p><p>Except() method requires two collections. It returns a new collection with elements from the first collection which do not exist in the second collection (parameter collection).</p>", "Negative Set: Distinct() and Except()"),
                new("Intersect and Union", "LinqSet2", "<p>The Intersect() extension method requires two collections. It returns a new collection that includes common elements that exists in both the collection. Consider the following example.</p><p>The Union() extension method requires two collections and returns a new collection that includes distinct elements from both the collections. Consider the following example.</p>", "Positive Sets: Intersect() and Union()"),
            },
            ResourceURLs = new Dictionary<string, string> { { "Language Integrated Query (LINQ)", "https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/" }, { "Work with Language-Integrated Query (LINQ)", "https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/working-with-linq" }, { "System.Linq Enumerable Class", "https://docs.microsoft.com/en-us/dotnet/api/system.linq.enumerable?view=net-5.0#methods" } }
        };
        public static readonly Dictionary<string, string> LinqFromGithubSnippets = new()
        {
            { "Linq Filter", "LinqFilter" },
            { "Linq Ordering", "LinqOrderBy" },
            { "Linq Aggregate", "LinqAggregate" },
            { "Linq with Numbers", "LinqMaths" },
            { "Distinct and Except", "LinqSet1" },
            { "Intersect and Union", "LinqSet2" }
        };

        #endregion


        public static readonly CodeSamples CollectionSamples = new()
        {
            Samples = new List<CodeSample>
            {
                new("Stack", STACK,"<p>Stack is a special type of collection that stores elements in LIFO style (Last In First Out). C# includes the generic <code>Stack<T></code> and non-generic <code>Stack</code>collection classes. It is recommended to use the generic Stack<T> collection.</p><p>Stack is useful to store temporary data in LIFO style, and you might want to delete an element after retrieving its value.</p><p>When you add an item in the list, it is called <code>pushing</code> the item and when you remove it, it is called <code>popping</code> the item.</p>","Stack<T>: Represents a variable size last-in-first-out (LIFO) collection of instances of the same specified type."),
                new("Queue", QUEUE,"<p>Elements can be added using the <code>Enqueue()</code> method. Cannot use collection-initializer syntax.</p><p>Elements can be retrieved using the <code>Dequeue()</code> and the <code>Peek()</code> methods. It does not support an indexer.</p>","Queue<T>: Represents a first-in, first-out collection of objects."),
                new("HashTable", HASHTABLE,"<p>A hash table is used when you need to access elements by using key, and you can identify a useful key value. Each item in the hash table has a key/value pair. The key is used to access the items in the collection.</p><p>It optimizes lookups by computing the hash code of each key and stores it in a different bucket internally and then matches the hash code of the specified key at the time of accessing values.</p>","Hashtable: Represents a collection of key/value pairs that are organized based on the hash code of the key."),
                new("List", LIST,"<p>The List<T> is a collection of strongly typed objects that can be accessed by index and having methods for sorting, searching, and modifying list. It is the generic version of the ArrayList that comes under System.Collection.Generic namespace.</p>", "List<T>: Represents a strongly typed list of objects that can be accessed by index. Provides methods to search, sort, and manipulate lists."),
                new("Dictionary", DICTIONARY,"<p>The <code>Dictionary<TKey, TValue></code> is a generic collection that stores <code>key-value pairs</code> in no particular order.</p>", "Dictionary<TKey, TValue>: Collection of unique key/value pairs"),
            },
            ResourceURLs = new Dictionary<string, string> { { "Collections (C#)", "https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/collections" }, { "Generics (C# Programming Guide)", "https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/" } }
        };
        public static readonly Dictionary<string, string> CollectionsSnippets = new()
        {
            {"ArrayList", ARRAYLIST}, {"Stack", STACK}, {"Queue", QUEUE},
            {"HashTable", HASHTABLE},{"List", LIST},{"Dictionary", DICTIONARY}
        };
        public static readonly CodeSamples StringSamples = new()
        {
            Samples = new List<CodeSample>
            {
                new("Concatenation", CONCATENATION,"<p>Concatenation is the process of appending one string to the end of another string. You concatenate strings by using the + operator.</p><p> For string literals and string constants, concatenation occurs at compile time; no run-time concatenation occurs. For string variables, concatenation occurs only at run time.</p>","Concatenation is the process of appending one string to the end of another string"),
                new("String.Format", FORMAT,"<p>Converts the value of objects to strings based on the formats specified and inserts them into another string.</p>","Convert and Insert to a new string"),
                new("Interpolation", INTERPOLATION,"<p>The $ special character identifies a string literal as an interpolated string. An interpolated string is a string literal that might contain interpolation expressions.</p><p> When an interpolated string is resolved to a result string, items with interpolation expressions are replaced by the string representations of the expression results.</p>","identified by the $ character, interpolation combines strings and expressions into a single string"),
                new("SubString", SUBSTRING,"<p>In C#, Substring() is a string method. It is used to retrieve a substring from the current instance of the string.</p><p> This method can be overloaded by passing the different number of parameters to it as follows:</p><p>String.Substring(Int32) - Substring(int startIndex) partial string from char at startIndex to end of the string.</p><p>String.Substring(Int32, Int32) Method Substring(int startIndex, int length) partial string from char at startIndex until the string end or reachs 'length' of chars</p>","string.Substring() is used to retrieve a substring from the current instance of the string."),
                new("Array to string", ARRAYTOSTRING,"<p>Using Join() Method: This method is used to concatenate the members of a collection or the elements of the specified array, using the specified separator between each member or element. It can be used to create a new string from the character array.</p><p> Syntax: string str = string.Join(string seperator, collection or object)</p>","A common way to merge a char[] into a string Type"),
                new("String to array", STRINGTOARRAY,"<p>In C#, ToCharArray() is a string method. This method is used to copy the characters from a specified string in the current instance to a Unicode character array or the characters of a specified substring in the current instance to a Unicode character array.</p> ","string.ToCharArray() turns a string into a char[]")
            },
            ResourceURLs = new Dictionary<string, string> { { "Strings (C# Programming Guide)", "https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/strings/" },{ "String Class", "https://docs.microsoft.com/en-us/dotnet/api/system.string?view=net-5.0" } }
        };
        public static readonly Dictionary<string, string> StringsSnippets = new()
        {
            {"Concatenation", CONCATENATION},{"String.Format", FORMAT},{"Interpolation", INTERPOLATION},{"SubString", SUBSTRING},{"Array to string", ARRAYTOSTRING},{"String to array", STRINGTOARRAY}
        };
        public static readonly CodeSamples ControlFlowSamples = new()
        {
            Samples = new List<CodeSample>
            {
                new("If Statement", IFCONDITIONAL,"The if statement contains a boolean condition followed by a single or multi-line code block to be executed. At runtime, if a boolean condition evaluates to true, then the code block will be executed, otherwise not.", "Single flow control"),
                new("If-Else Statement", IFELSE,"The else statement can come only after <code>if</code> or <code>else if</code> statement and can be used only once in the if-else statements. The else statement cannot contain any condition and will be executed when all the previous if and else if conditions evaluate to false.","Multiple flow control"),
                new("If-Else if-Else Statement", ELSEIF,"Multiple <code>else if</code> statements can be used after an <code>if</code> statement. It will only be executed when the if condition evaluates to false. So, either <code>if</code> or one of the <code>else if</code> statements can be executed, but not both.","Multiple flow control, with default"),
                new("Switch Statement", SWITCH,"<p>The switch statement can be used instead of <code>if else</code> statement when you want to test a variable against three or more conditions.</p><p>The switch statement starts with the <code>switch</code> keyword that contains a match expression or a variable in the bracket switch(match expression). The result of this match expression or a variable will be tested against conditions specified as cases, inside the curly braces <code> { }</code>. A case must be specified with the unique constant value and ends with the colon :.</p><p>The switch statement can also contain an optional <code>default</code> label. The default label will be executed if no cases executed. The <code>break</code>, <code>return</code>, or <code>goto</code> keyword is used to exit the program control from a <code>switch</code> case.</p>","Multiple flow control. Alternative to large if-else if chains")
            },
            ResourceURLs = new Dictionary<string, string> { { "Learn conditional logic with branch and loop statements", "https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/intro-to-csharp/branches-and-loops-local" }, { "Statements (C# Programming Guide)", "https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/statements" } }
        };

        public static readonly CodeSamples LoopStatements = new()
        {
            Samples = new List<CodeSample>()
            {
                new("While loop", WhileLoop,
                    "<p>The while statement executes a statement or a block of statements while a specified Boolean expression evaluates to true. Because that expression is evaluated before each execution of the loop, a while loop executes zero or more times. This differs from the do loop, which executes one or more times.</p><p>At any point within the while statement block, you can break out of the loop by using the break statement.</p><p></p><p>You can step directly to the evaluation of the while expression by using the continue statement. If the expression evaluates to true, execution continues at the first statement in the loop. Otherwise, execution continues at the first statement after the loop.</p><p>You can also exit a while loop by the goto, return, or throw statements.</p>",
                    "Executes 0 or more times"),
                new("Do-while loop", DoWhileLoop,
                    "<p>The do statement executes a statement or a block of statements while a specified Boolean expression evaluates to true. Because that expression is evaluated after each execution of the loop, a do-while loop executes one or more times. This differs from the while loop, which executes zero or more times.</p>",
                    "Executes 1 or more times"),
                new("For Loop", FORLOOP,
                    "<p>The for loop contains the following three optional sections, separated by a semicolon ; </p><p><code>Initializer</code> -- The initializer section is used to initialize a variable that will be local to a for loop and cannot be accessed outside loop. It can also be zero or more assignment statements, method call, increment, or decrement expression e.g., ++i or i++, and await expression.</p><p><code>Condition</code> -- The condition is a boolean expression that will return either true or false. If an expression evaluates to true, then it will execute the loop again; otherwise, the loop is exited.</p><p><code>Iterator</code> -- The iterator defines the incremental or decremental of the loop variable.</p>",
                    ""),
                new("Foreach Loop", FOREACHLOOP,
                    "<p>The foreach statement executes a statement or a block of statements for each element in an instance of the type</p><p>The in keyword used along with foreach loop is used to iterate over the iterable-item. The in keyword selects an item from the iterable-item on each iteration and store it in the variable element.</p><p>On first iteration, the first item of iterable-item is stored in element. On second iteration, the second element is selected and so on.</p><p>The number of times the foreach loop will execute is equal to the number of elements in the array or collection.</p>")
            },
            ResourceURLs = new Dictionary<string, string>
            {
                {
                    "Learn conditional logic with branch and loop statements",
                    "https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/intro-to-csharp/branches-and-loops-local"
                },
                {
                    "Statements (C# Programming Guide)",
                    "https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/statements"
                }
            }
        };
        public static readonly Dictionary<string, string> ConditionalSnippets = new()
        {
            {"If Statement", IFCONDITIONAL}, {"If-Else Statement", IFELSE}, {"If-Else if-Else Statement", ELSEIF},
            {"Switch Statement", SWITCH}, {"For Loop", FORLOOP}, {"Foreach Loop", FOREACHLOOP}
        };

        public static readonly Dictionary<string, string> ConsoleSnippets = new()
        {
            {"Console App", HelloWorld},{"Console with user input", ConsoleInput},{"Console multiple Writelines",ConsoleMutlitpeWrites}
        };
        public static readonly CodeSamples ExtensionSamples = new()
        {
            Samples = new List<CodeSample>
            {
                new("Extend String To Word Count", StringToWordCount,"<p>Extension methods enable you to \"add\" methods to existing types without creating a new derived type, recompiling, or otherwise modifying the original type. Extension methods are static methods, but they're called as if they were instance methods on the extended type. For client code written in C#, F# and Visual Basic, there's no apparent difference between calling an extension method and the methods defined in a type.</p><p>The most common extension methods are the LINQ standard query operators that add query functionality to the existing <code>System.Collections.IEnumerable</code> and <code>System.Collections.Generic.IEnumerable<T></code> types. To use the standard query operators, first bring them into scope with a <code>using System.Linq</code> directive. Then any type that implements IEnumerable<T> appears to have instance methods such as GroupBy, OrderBy, Average, and so on. You can see these additional methods in IntelliSense statement completion when you type \"dot\" after an instance of an IEnumerable<T> type such as List<T> or Array.</p>"),
                new("Extend Double to Description", DoubleToText,"<p>Extension methods enable you to \"add\" methods to existing types without creating a new derived type, recompiling, or otherwise modifying the original type. Extension methods are static methods, but they're called as if they were instance methods on the extended type. For client code written in C#, F# and Visual Basic, there's no apparent difference between calling an extension method and the methods defined in a type.</p><p>The most common extension methods are the LINQ standard query operators that add query functionality to the existing <code>System.Collections.IEnumerable</code> and <code>System.Collections.Generic.IEnumerable<T></code> types. To use the standard query operators, first bring them into scope with a <code>using System.Linq</code> directive. Then any type that implements IEnumerable<T> appears to have instance methods such as GroupBy, OrderBy, Average, and so on. You can see these additional methods in IntelliSense statement completion when you type \"dot\" after an instance of an IEnumerable<T> type such as List<T> or Array.</p>"),
                new("Extend String to Index list", StringToCharIndexList,"<p>Extension methods enable you to \"add\" methods to existing types without creating a new derived type, recompiling, or otherwise modifying the original type. Extension methods are static methods, but they're called as if they were instance methods on the extended type. For client code written in C#, F# and Visual Basic, there's no apparent difference between calling an extension method and the methods defined in a type.</p><p>The most common extension methods are the LINQ standard query operators that add query functionality to the existing <code>System.Collections.IEnumerable</code> and <code>System.Collections.Generic.IEnumerable<T></code> types. To use the standard query operators, first bring them into scope with a <code>using System.Linq</code> directive. Then any type that implements IEnumerable<T> appears to have instance methods such as GroupBy, OrderBy, Average, and so on. You can see these additional methods in IntelliSense statement completion when you type \"dot\" after an instance of an IEnumerable<T> type such as List<T> or Array.</p>"),
                new("Extend Generic and Shuffle", ShuffleGeneric,"<p>Extension methods enable you to \"add\" methods to existing types without creating a new derived type, recompiling, or otherwise modifying the original type. Extension methods are static methods, but they're called as if they were instance methods on the extended type. For client code written in C#, F# and Visual Basic, there's no apparent difference between calling an extension method and the methods defined in a type.</p><p>The most common extension methods are the LINQ standard query operators that add query functionality to the existing <code>System.Collections.IEnumerable</code> and <code>System.Collections.Generic.IEnumerable<T></code> types. To use the standard query operators, first bring them into scope with a <code>using System.Linq</code> directive. Then any type that implements IEnumerable<T> appears to have instance methods such as GroupBy, OrderBy, Average, and so on. You can see these additional methods in IntelliSense statement completion when you type \"dot\" after an instance of an IEnumerable<T> type such as List<T> or Array.</p>")
            },
            ResourceURLs = new Dictionary<string, string> { { "Extension Methods (C# Programming Guide)", "https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods" }, { "How to implement and call a custom extension method (C# Programming Guide)", "https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/how-to-implement-and-call-a-custom-extension-method" } }
        };
        public static readonly Dictionary<string, string> ExtensionSnippets = new()
        {
            {"Extend String To Word Count", StringToWordCount}, {"Extend Double to Description", DoubleToText},
            {"Extend String to Index list", StringToCharIndexList}, {"Extend Generic and Shuffle", ShuffleGeneric}

        };
       
        private const string ARRAYLIST =
            "ArrayList al = new ArrayList();\nal.Add(1);\nal.Add(\"Example\");\nal.Add(true);\nreturn al;";

        private const string STACK =
            "Stack<int> st = new Stack<int>();\nst.Push(1);\nst.Push(2);\nst.Push(3);\nreturn st;";

        private const string QUEUE = "Queue<int> qt = new Queue<int>();\nqt.Enqueue(1);\nqt.Enqueue(2);\nqt.Enqueue(3);\nreturn qt;";

        private const string HASHTABLE =
            "Hashtable ht = new Hashtable();\nht.Add(\"001\",\".Net\");\nht.Add(\"002\",\"c#\");\nht.Add(\"003\",\"ASP.Net\");\nreturn ht;";

        private const string LIST = "List<string> list = new List<string>();\nlist.Add(\"item 1\");\nlist.Add(\"item 2\");\nlist.Add(\"item 3\");\nreturn list;";
        private const string DICTIONARY = "Dictionary<int, string> dict = new Dictionary<int, string>();\ndict.Add(1, \"item 1\");\ndict.Add(2, \"item 2\");\ndict.Add(3, \"item 3\");\nreturn dict;";
        private const string CONCATENATION =
            "string nowDateTime = \"Date: \" + DateTime.Now.ToString(\"D\");\n" +
            "string firstName = \"Gob\";\nstring lastName = \"Bluth\";\nstring age = \"33\";\n" + 
            "string authorDetails = firstName + \" \" + lastName + \" is \" + age + \" years old.\";\nreturn authorDetails;";
        private const string FORMAT = "string name = \"George Bluth\";\nint age = 33;\n" +
                                      "string authorInfo = string.Format(\"{0} is {1} years old.\", name, age.ToString());\nreturn authorInfo;";
        private const string INTERPOLATION = "string name = \"George Bluth\";\nint age = 33;\n" +
                                             "string authorInfo = string.Format($\"{name} is {age} years old.\");\nreturn authorInfo;";
        private const string SUBSTRING = "string authorInfo = \"Buster Bluth is 33 years old.\";\n" +
                                         "int startPosition = authorInfo.IndexOf(\"is \") + 1;\n" +
                                         "string age = authorInfo.Substring(startPosition +2, 2 );\nreturn age;";

        private const string ARRAYTOSTRING =
            "char[] chars = { 'C', 'S', 'h', 'a', 'r', 'p' };\nstring name = new string(chars);\nreturn name;";

        private const string STRINGTOARRAY = "string sentence = \"Adam Holm is the author and founder of mastercsharp\";\n" +
                                             "char[] charArr = sentence.ToCharArray();\nreturn charArr;";

        private const string IFCONDITIONAL = "char[] chars = { 'C', 'S', 'h', 'a', 'r', 'p' };\nstring name = new string(chars);\nif (name.Length > 5)\n{\n\tname = $\"{name} is sometimes easy\";\n}\nreturn name;";
        private const string IFELSE = "char[] chars = { 'C', 'S', 'h', 'a', 'r', 'p' };\nstring name = new string(chars);\nif (name.Length > 6)\n{\n\tname = $\"{name} is sometimes easy\";\n}\nelse\n{\n\tname = $\"{name} is sometimes hard\";\n}\nreturn name;\n";
        private const string ELSEIF = "char[] chars = { 'C', 'S', 'h', 'a', 'r', 'p' };\nstring name = new string(chars);\nint nameLength = name.Length;\nif (nameLength > 7)\n{\n\tname = $\"{name} is sometimes easy\";\n}\nelse if (nameLength > 6)\n{\n\tname = $\"{name} is sometimes hard\";\n}\nelse{\n\tname = $\"{name} is always c#\";\n}\nreturn name;";
        private const string SWITCH = "char[] chars = { 'C', 'S', 'h', 'a', 'r', 'p' };\nstring name = new string(chars);\nint nameLength = name.Length;\nswitch (nameLength)\n{\n\tcase 7:\n\t\tname = $\"{name} is sometimes easy\";\n\t\tbreak;\n\tcase 8:\n\t\tname = $\"{name} is sometimes hard\";\n\t\tbreak;\n\tdefault:\n\t\tname = $\"{name} is always c#\";\n\t\tbreak;\n}\nreturn name;";
        private const string FORLOOP = "char[] chars = { 'C', 'S', 'h', 'a', 'r', 'p' };\nstring name = new string(chars);\nfor (int i = 0; i < chars.Length; i++)\n{\n\tname = name + \"!\";\n}\nreturn name;";
        private const string FOREACHLOOP = "char[] chars = { 'C', 'S', 'h', 'a', 'r', 'p' };\nstring name = \"\";\nforeach (var str in chars)\n{\n\tname += str;\n}\nreturn name;\n";

        private const string WhileLoop = @"int n = 0;
while (n < 5)
{
    Console.WriteLine(n);
    n++;
}";

        private const string DoWhileLoop = @"int n = 0;
do{
Console.WriteLine(n);
    n++;
}
while (n < 5);";
        private const string HelloWorld = "using System;\n\nclass Program\n{\n\tpublic static void Main()\n\t{\n\t\tConsole.WriteLine(\"Hello World\");\n\t}\n}";

        private const string ConsoleInput = "using System;\n\nclass Program\n{\n\tpublic static void Main()\n\t{\n\t\tstring input = Console.ReadLine();\n\t\tstring additional = \" was your input\";\n\t\tConsole.WriteLine(input);\n\t\tstring newOutput = input + additional;\n\t\tConsole.WriteLine(newOutput);\n\t}\n}";

        private const string ConsoleMutlitpeWrites =
            "using System;\n\nclass Program\n{\n\tpublic static void Main()\n\t{\n\t\tstring input = \"Foo\";\n\t\tstring additional = \" was not your input\";\n\t\tConsole.WriteLine(input);\n\t\tstring newOutput = input + additional;\n\t\tConsole.WriteLine(newOutput);\n\t}\n}";

        private const string ShuffleGeneric = "public static void Shuffle<T>(this IList<T> cards)\n{\n\tvar rng = new Random();\n\tint n = cards.Count;\n\twhile (n > 1)\n\t{\n\t\tn--;\n\t\tint k = rng.Next(n + 1);\n\t\tT value = cards[k];\n\t\tcards[k] = cards[n];\n\t\tcards[n] = value;\n\t}\n}\n\nvar list = new List<int> {1,2,3,4,5,6,7,8,9,10,11,12,13,14,15};\nlist.Shuffle();\nreturn list;";
        private const string DoubleToText = "public static string ExtDoubleToText(this double inp)\n{\n\tif (inp > .8)\n\t\treturn \"very likely\";\n\tif (inp > .5 && inp <= .8)\n\t\treturn \"somewhat likely\";\n\tif (inp > .25 && inp <= .5)\n\t\treturn \"somewhat unlikely\";\n\treturn \"very unlikely\";\n}\nvar myodds = .7;\nreturn myodds.ExtDoubleToText();";
        private const string StringToWordCount = "public static int WordCount(this string words)\n{\n\tvar stringToArray = words.Split(' ');\n\tvar wordCount = stringToArray.Length;\n\treturn wordCount;\n}\nreturn \"How many words are here?\".WordCount();";

        private const string StringToCharIndexList =
            "public static List<int> AllIndexesOf(this string str, string value)\n{\n\tif (string.IsNullOrEmpty(value))\n\t\treturn new List<int>();\n\tvar indexes = new List<int>();\n\tfor (int index = 0; ; index += value.Length)\n\t{\n\t\tindex = str.IndexOf(value, index, StringComparison.Ordinal);\n\t\tif (index == -1)\n\t\t\treturn indexes;\n\t\tindexes.Add(index);\n\t}\n}\nreturn \"i like it\".AllIndexesOf(\"i\");";

        public const string DefaultCode = @"private string MyProgram() 
{
    string input = ""this does not"";
    string modify = input + "" suck!"";
    return modify;
}
return MyProgram();";


    }
}
