﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.2.0.0
//      SpecFlow Generator Version:2.2.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace BullOak.Repositories.EventStore.Test.Integration.Specification
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.2.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class SaveEventsStreamFeature : Xunit.IClassFixture<SaveEventsStreamFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "SaveEventStream.feature"
#line hidden
        
        public SaveEventsStreamFeature(SaveEventsStreamFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "SaveEventsStream", "\tIn order to persist using an event stream\r\n\tAs a developer usint this new librar" +
                    "y\r\n\tI want to be able to save events in a stream", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void TestInitialize()
        {
        }
        
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        void System.IDisposable.Dispose()
        {
            this.ScenarioTearDown();
        }
        
        [Xunit.TheoryAttribute(DisplayName="Save events in a new stream")]
        [Xunit.TraitAttribute("FeatureTitle", "SaveEventsStream")]
        [Xunit.TraitAttribute("Description", "Save events in a new stream")]
        [Xunit.InlineDataAttribute("1", new string[0])]
        [Xunit.InlineDataAttribute("30", new string[0])]
        [Xunit.InlineDataAttribute("10000", new string[0])]
        public virtual void SaveEventsInANewStream(string eventsCount, string[] exampleTags)
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Save events in a new stream", exampleTags);
#line 6
this.ScenarioSetup(scenarioInfo);
#line 7
 testRunner.Given("a new stream", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 8
 testRunner.And(string.Format("{0} new events", eventsCount), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 9
 testRunner.When("I try to save the new events in the stream", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 10
 testRunner.Then("the save process should succeed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 11
  testRunner.And(string.Format("there should be {0} events in the stream", eventsCount), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Save one event using interface")]
        [Xunit.TraitAttribute("FeatureTitle", "SaveEventsStream")]
        [Xunit.TraitAttribute("Description", "Save one event using interface")]
        public virtual void SaveOneEventUsingInterface()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Save one event using interface", ((string[])(null)));
#line 18
this.ScenarioSetup(scenarioInfo);
#line 19
 testRunner.Given("a new stream", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 20
 testRunner.And("1 new event", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 21
 testRunner.When("I try to save the new events in the stream through their interface", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 22
 testRunner.Then("the save process should succeed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Save additional events in an existing stream")]
        [Xunit.TraitAttribute("FeatureTitle", "SaveEventsStream")]
        [Xunit.TraitAttribute("Description", "Save additional events in an existing stream")]
        public virtual void SaveAdditionalEventsInAnExistingStream()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Save additional events in an existing stream", ((string[])(null)));
#line 24
this.ScenarioSetup(scenarioInfo);
#line 25
 testRunner.Given("an existing stream with 10 events", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 26
 testRunner.And("10 new events", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 27
 testRunner.When("I try to save the new events in the stream", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 28
 testRunner.Then("the save process should succeed", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 29
 testRunner.And("there should be 20 events in the stream", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Concurrent write should fail for outdated session")]
        [Xunit.TraitAttribute("FeatureTitle", "SaveEventsStream")]
        [Xunit.TraitAttribute("Description", "Concurrent write should fail for outdated session")]
        public virtual void ConcurrentWriteShouldFailForOutdatedSession()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Concurrent write should fail for outdated session", ((string[])(null)));
#line 31
this.ScenarioSetup(scenarioInfo);
#line 32
 testRunner.Given("an existing stream with 10 events", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 33
 testRunner.And("session \'Session1\' is open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 34
 testRunner.And("session \'Session2\' is open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 35
 testRunner.And("10 new events are added by \'Session1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 36
 testRunner.And("10 new events are added by \'Session2\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 37
 testRunner.When("I try to save \'Session1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 38
 testRunner.And("I try to save \'Session2\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 39
 testRunner.Then("the save process should succeed for \'Session1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 40
 testRunner.And("the save process should fail for \'Session2\' with ConcurrencyException", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 41
 testRunner.And("there should be 20 events in the stream", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [Xunit.FactAttribute(DisplayName="Saving already saved session should throw meaningful usage advice exception")]
        [Xunit.TraitAttribute("FeatureTitle", "SaveEventsStream")]
        [Xunit.TraitAttribute("Description", "Saving already saved session should throw meaningful usage advice exception")]
        public virtual void SavingAlreadySavedSessionShouldThrowMeaningfulUsageAdviceException()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Saving already saved session should throw meaningful usage advice exception", ((string[])(null)));
#line 43
this.ScenarioSetup(scenarioInfo);
#line 44
 testRunner.Given("an existing stream with 10 events", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line 45
 testRunner.And("session \'Session1\' is open", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 46
 testRunner.And("10 new events are added by \'Session1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 47
 testRunner.When("I try to save \'Session1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 48
 testRunner.And("I try to save \'Session1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 49
 testRunner.Then("the save process should fail for \'Session1\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 50
 testRunner.And("there should be 20 events in the stream", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.2.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                SaveEventsStreamFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                SaveEventsStreamFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
