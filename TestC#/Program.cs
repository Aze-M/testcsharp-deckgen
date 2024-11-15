
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

main();

static void main()
{

    Random rand = new Random();
    ThreadLocal<Random> tl_rand = new ThreadLocal<Random>(() => new Random());

    //depricated
    void randomize_list(List<Int32> _list)
    {
        Int32 _range = _list.Count();
        List<Int32> _options = new List<Int32>();

        // populate options list with numbers up to the element count in the list
        for (int i = 0; i < _range; i++)
        {
            _options.Add(i);
        }

        Int32 _id1 = 0;
        Int32 _id2 = 0;
        Int32 _temp = 0;

        while (_options.Count() > 1)
        {
            //get the first half of swap pair, remove it from the list
            _id1 = _options[rand.Next(_options.Count())];
            _options.Remove(_id1);

            // and the second, remove it too
            _id2 = _options[rand.Next(_options.Count())];
            _options.Remove(_id2);

            _temp = _list[_id1];
            _list[_id1] = _list[_id2];
            _list[_id2] = _temp;

        }

    }

    void randomize_list_fisher_yates(List<Int32> _list) 
    {
        //get total element count
        int _count = _list.Count;

        //start at one before the end
        for (int _id1 = _count - 1; _id1 > 0; _id1--)
        {
            //randomly pick an element including the current one
            int _id2 = rand.Next(_id1 + 1);

            //swap them
            int _temp = _list[_id1];
            _list[_id1] = _list[_id2];
            _list[_id2] = _temp;
        }
    }

    void make_deck(List<Card> _deck)
    {
        for(int _suit = 0; _suit < 4; _suit++)
        {
            for(int _value = 0; _value < 13; _value++)
            {
                Card _card = new Card(_suit, _value);
                _deck.Add(_card);
            }
        }
    }

    void make_deck_bytearr(byte[] _deck)
    {
        int cardtotal = 0;

        for (int suit = 0; suit < 4; suit++)
        {
            for (int value = 0; value < 13; value++)
            {
                _deck[cardtotal] = (byte)((suit << 4) | value);
                cardtotal++;
            }
        }
    }

    void shuffle_deck(List<Card> _deck)
    {
        int _cards = _deck.Count;

        for (int _id = _cards - 1; _id > 0; _id--)
        {
            int _id2 = rand.Next(_id + 1);

            Card _temp = _deck[_id];
            _deck[_id] = _deck[_id2];
            _deck[_id2] = _temp;
        }
    }

    void shuffle_deck_bytearr(byte[] _deck)
    {
        int _cards = _deck.Length;

        for (int id = 0; id < _cards; id++)
        {

            int id2 = rand.Next(id + 1);

            byte temp = _deck[id];
            _deck[id] = _deck[id2];
            _deck[id2] = temp;

        }
    }

    void shuffle_deck_bytearr_thread(byte[] _deck)
    {
        int _cards = _deck.Length;

        Random localrand = tl_rand.Value;

        for (int id = 0; id < _cards; id++)
        {

            int id2 = localrand.Next(id + 1);

            byte temp = _deck[id];
            _deck[id] = _deck[id2];
            _deck[id2] = temp;

        }
    }

    bool valid_deck(List<Card> _deck)
    {
        int[] suitCounts = new int[4];

        if (_deck.Count != 52)
        {
            return false;
        }

        foreach (Card _card in _deck)
        {
            int suit = _card.getsuit();

            suitCounts[suit]++;

            if (suitCounts[suit] > 13)
            {
                return false;
            }
        }

        return true;
    }

    bool valid_deck_bytearr(byte[] _deck)
    {
        int[] suitCounts = new int[4];

        if (_deck.Length != 52)
        {
            return false;
        }

        foreach (byte _card in _deck)
        {
            int suit = _card >> 4;

            suitCounts[suit]++;

            if (suitCounts[suit] > 13)
            {
                    return false;
            }
        }

        return true;
    }

    string make_deck_hash(List<Card> _deck)
    {
        StringBuilder _out = new StringBuilder();

        foreach (Card _card in _deck)
        {
            // 8 1 is 1000 and 0001 turns into 1000 0000 0001
            int cardval = (_card.getsuit() << 4) | _card.getvalue();

            _out.Append(cardval.ToString("X2"));
        }

        return _out.ToString();
        
    }

    BigInteger make_deck_hash_shift(List<Card> _deck)
    {
        BigInteger _out = 0;

        foreach (Card _card in _deck)
        {
            _out = _out | _card.getsuit();
            _out = _out << 4;
            _out = _out | _card.getvalue();
            _out = _out << 2;
        }

        return _out;
    }

    BigInteger make_deck_hash_shift_51(List<Card> _deck)
    {
        BigInteger _out = 0;

        for (int idx = 0; idx < _deck.Count -1; idx++)
        {
            _out = _out | _deck[idx].getsuit();
            _out = _out << 4;
            _out = _out | _deck[idx].getvalue();
            _out = _out << 2;
        }

        _out = _out | _deck[51].getsuit();
        _out = _out << 4;
        _out = _out | _deck[51].getvalue();

        return _out;
    }

    byte[] make_deck_hash_bytearr(List<Card> _deck)
    {
        byte[] _out = new byte[39];
        int _bitpos = 0;

        foreach (Card card in _deck)
        {
            int cardBits = (card.getsuit() << 4) | card.getvalue();

            int byteidx = _bitpos / 8;
            int bitoset = _bitpos % 8;

            _out[byteidx] |= (byte)(cardBits << bitoset);

            if (bitoset > 2) // ex 12 % 8 = 4, 18 % 8 = 2, 24 % 8 = 0, only if we aren't on an exact 2 bytes we do not offset
            {
                _out[byteidx + 1] |= (byte)(cardBits >> (8 - bitoset));
            }

            _bitpos += 6; // move bit pos by 6
        }

        return _out;
    }

    byte[] make_deck_bytearr_hash_bytearr(byte[] _deck)
    {
        byte[] _out = new byte[39];
        int _bitpos = 0;

        foreach (byte cardBits in _deck)
        {

            int byteidx = _bitpos / 8;
            int bitoset = _bitpos % 8;

            _out[byteidx] |= (byte)(cardBits << bitoset);

            if (bitoset > 2) // ex 12 % 8 = 4, 18 % 8 = 2, 24 % 8 = 0, only if we aren't on an exact 2 bytes we do not offset
            {
                _out[byteidx + 1] |= (byte)(cardBits >> (8 - bitoset));
            }

            _bitpos += 6; // move bit pos by 6
        }

        return _out;
    }

    /*
    List<Int32> list = new List<Int32>() {1,2,3,4,5,6,7,8,9,0};
    List<Int32> list_fy = new List<Int32>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
    

    List<Card> deck = new List<Card>();

    make_deck(deck);
    shuffle_deck(deck);
    //deck.Add(new Card(1, 1));

    
    randomize_list(list);
    randomize_list_fisher_yates(list_fy);
    */

    //HashSet<BigInteger> decks_int = new HashSet<BigInteger>();
    HashSet<byte[]> decks_bytes = new HashSet<byte[]>();
    HashSet<string> decks_strd = new HashSet<string>();
    bool looping = true;
    ReaderWriterLockSlim mex = new ReaderWriterLockSlim();

    /*
    while (looping) { 

        bool valid = valid_deck(deck);
        Console.WriteLine("Generated deck is valid: " + valid);
    
        if (!valid)
        {
            Console.WriteLine("Invalid Deck, Exiting.");
            System.Environment.Exit(9);
        }
        
    
        for (int i = 0; i < deck.Count; i++)
        {
            Console.WriteLine((i + 1) + ": " + deck[i].printable());
        }
    

        string deck_hash = make_deck_hash(deck);
        if (decks.Contains(deck_hash))
        {
            Console.WriteLine("Generated Deck is not unique! Exiting!");
            System.Environment.Exit(0);
        } else
        {
            Console.WriteLine("Generated Deck is Unique! Continuing!");
            decks.Add(deck_hash);
            shuffle_deck(deck);
        }
        
    }
    */

    void deck_check_threaded(ReaderWriterLockSlim _lock, bool looping, int thread_id)
    {
        
        List<Card> deck = new List<Card> ();
        make_deck(deck);
        shuffle_deck(deck);

        while (looping)
        {

            bool valid = valid_deck(deck);

            if (!valid)
            {
                Console.WriteLine("Invalid Deck, Exiting.");
                System.Environment.Exit(9);
            }

            byte[] deck_hash = make_deck_hash_bytearr(deck);
            if (decks_bytes.Contains(deck_hash))
            {
                Console.WriteLine("Duplicated Deck! Exiting!");
                looping = false;
                System.Environment.Exit(0);
            }
            else
            {
                
                _lock.EnterWriteLock();
                decks_bytes.Add(deck_hash);
                _lock.ExitWriteLock();

                shuffle_deck(deck);
                
            }

        }
    }

    void deck_check_threaded_bytearr(ReaderWriterLockSlim _lock, bool looping, int thread_id)
        {

            byte[] deck = new byte[52];
            make_deck_bytearr(deck);
            shuffle_deck_bytearr(deck);

            while (looping)
            {

                bool valid = valid_deck_bytearr(deck);

                if (!valid)
                {
                    Console.WriteLine("Invalid Deck, Exiting.");
                    System.Environment.Exit(9);
                }

                byte[] deck_hash = make_deck_bytearr_hash_bytearr(deck);
                if (decks_bytes.Contains(deck_hash))
                {
                    Console.WriteLine("Duplicated Deck! Exiting!");
                    looping = false;
                    System.Environment.Exit(0);
                }
                else
                {

                    _lock.EnterWriteLock();
                    decks_bytes.Add(deck_hash);
                    _lock.ExitWriteLock();

                    shuffle_deck_bytearr_thread(deck);

                }

            }
        }

    /*
    void deck_check_thread_barr_dict(bool looping, int thread_id)
    {

        byte[] deck = new byte[52];
        make_deck_bytearr(deck);
        shuffle_deck_bytearr(deck);

        while (looping)
        {

            bool valid = valid_deck_bytearr(deck);

            if (!valid)
            {
                Console.WriteLine("Invalid Deck, Exiting.");
                System.Environment.Exit(9);
            }

            byte[] deck_hash = make_deck_bytearr_hash_bytearr(deck);
            if (decks_byte_dict.ContainsKey(deck_hash))
            {
                Console.WriteLine("Duplicated Deck! Exiting!");
                looping = false;
                System.Environment.Exit(0);
            }
            else
            {

                decks_byte_dict[deck_hash] = true;
                shuffle_deck_bytearr(deck);

            }

        }
    }
    */

    int max_threads = Environment.ProcessorCount;
    List<Thread> threads = new List<Thread>();

    for (int i = 0; i < max_threads; i++)
    {
        Thread _cur_thread = new Thread(() => deck_check_threaded_bytearr(mex, looping, i));
        _cur_thread.Start();
        threads.Add(_cur_thread);
    }

    foreach (Thread thread in threads) {
        thread.Join();
    }

    Console.WriteLine("All threads have exited. {0}", decks_bytes.Count);

};
struct Card
{
    private int _suit;
    private int _value;


    internal void setsuit(int _suit)
    {
        this._suit = _suit;
    }

    internal void setvalue(int _value)
    {
        this._value = _value;
    }

    internal int getsuit()
    {
        return this._suit;
    }

    internal int getvalue()
    {
        return this._value;
    }

    public Card(int _suit, int _value)
    {
        if (_suit > 3 || _value > 13)
        {
            throw new ArgumentOutOfRangeException();
        }

        this._suit = _suit;
        this._value = _value;
    }

    internal string printable()
    {
        string[] suits = { "Clubs", "Spades", "Hearts", "Diamonds" };
        string[] values = { "Ace", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King" };

        StringBuilder _out = new StringBuilder();

        _out.Append(values[this._value]);
        _out.Append(" of ");
        _out.Append(suits[this._suit]);

        return _out.ToString();
    }
}

