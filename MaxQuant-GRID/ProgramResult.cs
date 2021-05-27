namespace MaxQuantTaskCore
{
    internal class ProgramResult
    {
        public string Command { get; set; }
        public string StdOut { get; set; }
        public string StdErr { get; set; }
        public int ExitCode { get; set; }
    }
}