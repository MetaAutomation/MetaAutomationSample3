////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//  MetaAutomation (C) 2016 by Matt Griscom.
//
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace CheckMethods
{
    using MetaAutomationClientMtLibrary;
    using System;

    internal class FaultGenerator
    {
        private static FaultGenerator m_TheInstance = null;
        private static Random m_Random = null;

        private FaultGenerator()
        {
            m_Random = new Random();
        }

        public static FaultGenerator Instance
        {
            get
            {
                if (FaultGenerator.m_TheInstance == null)
                {
                    FaultGenerator.m_TheInstance = new FaultGenerator();
                }

                return FaultGenerator.m_TheInstance;
            }
        }

        public void ThrowOnProbability(double probability, Type typeToThrow)
        {
            if (probability < 0)
            {
                throw new CheckInfrastructureClientException("The given probability parameter is negative. Acceptable range: 0 <= probability <= 1.");
            }

            if (probability > 1)
            {
                throw new CheckInfrastructureClientException("The given probability parameter is greater than one. Acceptable range: 0 <= probability <= 1.");
            }

            if (!typeToThrow.IsSubclassOf(typeof(System.Exception)))
            {
                throw new CheckInfrastructureClientException(string.Format("The passed type '{0}' must derive from System.Exception.", typeToThrow.Name));
            }

            double randomNumber = m_Random.NextDouble();

            // Decide whether to throw an exception or not
            if (probability > randomNumber)
            {
                string message = string.Format("This exception of type '{0}' is thrown intentionally on given probability '{1}'", typeToThrow.Name, probability.ToString("0.000"));
                System.Exception exceptionObject = (System.Exception) Activator.CreateInstance(typeToThrow, new string[] { message });
                throw exceptionObject;
            }
        }
    }
}
