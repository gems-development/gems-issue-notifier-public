using Microsoft.FeatureManagement;
using Moq;
using static Gems.TechSupport.Application.Commands.Okdesk.Constants;

namespace Gems.TechSupport.Application.Test.HelperMethods
{
    public static class FeatureManagerSetup
    {
        public static void SetupIsEnabledAsync(Mock<IFeatureManager> featureManagerMock, bool isEnabled)
        {
            var turnOnSkitProcessing = OkdeskFeatures.SkitIssuesProcessing;
            featureManagerMock
                .Setup(f => f.IsEnabledAsync(turnOnSkitProcessing))
                .ReturnsAsync(isEnabled);
        }
    }
}
