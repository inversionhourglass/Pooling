using System;

namespace SingleFeatureCases.Cases.ILInspects
{
    public class ExceptionFilter
    {
        public static void M()
        {
            try
            {
                Console.WriteLine(12345);
            }
            catch (Exception e) when (e.Message == "12345")
            {
                Console.WriteLine(e);
            }
            catch (Exception e) when (e.Data.Count != 0)
            {
                Console.WriteLine(e.Data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                Console.WriteLine(67890);
            }
        }
    }
}
