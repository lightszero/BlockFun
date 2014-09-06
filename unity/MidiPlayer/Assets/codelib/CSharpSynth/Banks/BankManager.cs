using System.Collections.Generic;
using System.IO;

namespace CSharpSynth.Banks
{
    public static class BankManager
    {
        //--Variables
        private static List<InstrumentBank> _banks = new List<InstrumentBank>();
        public const int DEFAULT_BANK_SIZE = 256; //midi standard only needs 0-127. The rest is extra space.
        public const int DEFAULT_DRUMBANK_SIZE = 128;
        //--Static Properties
        public static int Count
        {
            get { return _banks.Count; }
        }
        public static List<InstrumentBank> Banks
        {
            get { return _banks; }
        }
        //--Public Static Methods
        public static void addBank(InstrumentBank bank)
        {
            _banks.Add(bank);
        }
        public static void removeBank(int index)
        {
            _banks[index].Clear();
            _banks.RemoveAt(index);
        }
        public static void removeBank(InstrumentBank bank)
        {
            int index = _banks.IndexOf(bank);
            if (index > -1)
                removeBank(index);
        }
        public static void removeBank(string bankname)
        {
            InstrumentBank bank = getBank(bankname);
            if (bank != null)
                removeBank(bank);
        }
        public static int getBankIndex(string bankname)
        {
            bankname = Path.GetFileName(bankname).ToLower();
            for (int x = 0; x < _banks.Count; x++)
            {
                if (Path.GetFileName(_banks[x].BankPath).ToLower().Equals(bankname))
                    return x;
            }
            return -1;
        }
        public static int getBankIndex(InstrumentBank bank)
        {
            return _banks.IndexOf(bank);
        }
        public static InstrumentBank getBank(int index)
        {
            return _banks[index];
        }
        public static InstrumentBank getBank(string bankname)
        {
            int index = getBankIndex(bankname);
            if (index > -1)
                return _banks[index];
            return null;
        }
        public static void Clear()
        {
            for (int x = 0; x < _banks.Count; x++)
            {
                _banks[x].Clear();
            }
            _banks.Clear();
        }
    }
}
