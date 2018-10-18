namespace Tests
{
    public interface ITest
    {
        bool test();
    }
    public class Test : ITest
    {
        public Test()
        {
            System.Console.WriteLine("Creating instance of Test");
        }
        public bool test()
        {
            bool i = TestA.testing();
            return i;
        }
    }
}
