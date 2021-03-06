﻿using System;
using System.Linq;
using NLog.xLogger;
using Utility.OperationResult;
using OpenIIoT.SDK.Common;
using OpenIIoT.SDK;
using OpenIIoT.SDK.Configuration;
using OpenIIoT.SDK.Plugin;
using OpenIIoT.SDK.Plugin.Endpoint;

namespace OpenIIoT.Plugin.Endpoint.Example
{
    /// <summary>
    ///     This class is an example of an Endpoint.
    ///
    ///     Each Endpoint file should contain two classes; the Endpoint class and a class representing the model for the
    ///     configuration of the Endpoint. If no configuration is needed, or if it is very basic, a simple object can be
    ///     substituted for this configuration class.
    ///
    ///     All Endpoint classes must implement the IEndpoint and IConfigurable(T) interfaces, where T is the type of the
    ///     configuration class for the Endpoint.
    /// </summary>
    public class ExampleEndpoint : IEndpoint, IConfigurable<ExampleEndpointConfiguration>
    {
        #region Private Fields

        /// <summary>
        ///     The logger for the Endpoint.
        /// </summary>
        private xLogger logger;

        /// <summary>
        ///     The ApplicationManager for the application.
        /// </summary>
        private IApplicationManager manager;

        #endregion Private Fields

        #region Public Constructors

        public ExampleEndpoint(IApplicationManager manager, string instanceName, xLogger logger)
        {
            this.manager = manager;
            InstanceName = instanceName;
            this.logger = logger;
        }

        #endregion Public Constructors

        #region Public Events

        /// <summary>
        ///     Fired when the State property changes.
        /// </summary>
        public event EventHandler<StateChangedEventArgs> StateChanged;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        ///     The Configuration property is the type of the model/configuration class. This corresponds to the value of T in IConfigurable(T).
        /// </summary>
        public ExampleEndpointConfiguration Configuration { get; private set; }

        /// <summary>
        ///     The ConfigurationDefinition property returns the Endpoint's configuration details.
        ///
        ///     A ConfigurationDefinition instance includes two strings; a Form and a Schema, and a Type corresponding to the
        ///     model/configuration class.
        /// </summary>
        public ConfigurationDefinition ConfigurationDefinition { get { return GetConfigurationDefinition(); } }

        public string Fingerprint { get; private set; }

        /// <summary>
        ///     The Connector FQN.
        /// </summary>
        public string FQN { get; private set; }

        /// <summary>
        ///     The name of the Connector instance.
        /// </summary>
        public string InstanceName { get; private set; }

        /// <summary>
        ///     The Connector name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     The Connector type.
        /// </summary>
        public PluginType PluginType { get; private set; }

        /// <summary>
        ///     The State of the Connector.
        /// </summary>
        public State State { get; private set; }

        /// <summary>
        ///     The Connector Version.
        /// </summary>
        public string Version { get; private set; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        ///     The GetConfigurationDefinition method is static and returns the ConfigurationDefinition for the Endpoint.
        ///
        ///     This method is necessary so that the configuration defintion can be registered with the ConfigurationManager prior
        ///     to any instances being created. This method MUST be implemented, however it is not possible to specify static
        ///     methods in an interface, so implementing IConfigurable will not enforce this.
        /// </summary>
        /// <returns>The ConfigurationDefinition for the Endpoint.</returns>
        public static ConfigurationDefinition GetConfigurationDefinition()
        {
            ConfigurationDefinition retVal = new ConfigurationDefinition();

            // to create the form and schema strings, visit http://schemaform.io/examples/bootstrap-example.html use the example to
            // create the desired form and schema, and ensure that the resulting model matches the model for the endpoint. When you
            // are happy with the json from the above url, visit http://www.freeformatter.com/json-formatter.html#ad-output and
            // paste in the generated json and format it using the "JavaScript escaped" option. Paste the result into the methods below.

            retVal.Form = "[\"templateURL\",{\"type\":\"submit\",\"style\":\"btn-info\",\"title\":\"Save\"}]";
            retVal.Schema = "{\"type\":\"object\",\"title\":\"XMLEndpoint\",\"properties\":{\"templateURL\":{\"title\":\"Template URL\",\"type\":\"string\"}},\"required\":[\"templateURL\"]}";

            // this will always be typeof(YourConfiguration/ModelObject)
            retVal.Model = (typeof(ExampleEndpointConfiguration));
            return retVal;
        }

        /// <summary>
        ///     The GetDefaultConfiguration method is static and returns a default or blank instance of the confguration model/type.
        ///
        ///     If the ConfigurationManager fails to retrieve the configuration for an instance it will invoke this method and
        ///     return this value in lieu of a loaded configuration. This is a failsafe in case the configuration file becomes corrupted.
        /// </summary>
        /// <returns></returns>
        public static ExampleEndpointConfiguration GetDefaultConfiguration()
        {
            ExampleEndpointConfiguration retVal = new ExampleEndpointConfiguration();
            retVal.Example = "Hello World!  This is the example configuration.";
            return retVal;
        }

        /// <summary>
        ///     The parameterless Configure() method calls the overloaded Configure() and passes in the instance of the model/type
        ///     returned by the GetConfiguration() method in the Configuration Manager.
        ///
        ///     This is akin to saying "configure yourself using whatever is in the config file"
        /// </summary>
        /// <returns></returns>
        public Result Configure()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     The Configure method is called by external actors to configure or re-configure the Endpoint instance.
        ///
        ///     If anything inside the Endpoint needs to be refreshed to reflect changes to the configuration, do it in this method.
        /// </summary>
        /// <param name="configuration">The instance of the model/configuration type to apply.</param>
        /// <returns>An OperationResult containing the result of the operation.</returns>
        public Result Configure(ExampleEndpointConfiguration configuration)
        {
            Configuration = configuration;

            return new Result();
        }

        /// <summary>
        ///     Returns true if any of the specified <see cref="State"/> s match the current <see cref="State"/>.
        /// </summary>
        /// <param name="states">The list of States to check.</param>
        /// <returns>True if the current State matches any of the specified States, false otherwise.</returns>
        public virtual bool IsInState(params State[] states)
        {
            return states.Any(s => s == State);
        }

        public Result Restart(StopType stopType = StopType.Stop)
        {
            Guid guid = logger.EnterMethod(true);

            Result retVal = Start().Incorporate(Stop(stopType | StopType.Restart));

            retVal.LogResult(logger);
            logger.ExitMethod(retVal, guid);
            return retVal;
        }

        public Result SaveConfiguration()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     The Send method sends the supplied value to the configured Endpoint.
        /// </summary>
        /// <param name="value">A generic object containing the value to send to the Endpoint.</param>
        /// <returns>An OperationResult containing the result of the operation.</returns>
        public Result Send(object value)
        {
            throw new NotImplementedException();
        }

        public void SetFingerprint(string fingerprint)
        {
            Fingerprint = fingerprint;
        }

        public Result Start()
        {
            Guid guid = logger.EnterMethod(true);

            Result retVal = new Result();
            ChangeState(State.Starting);

            try
            {
                // todo: implement startup logic
            }
            catch (Exception ex)
            {
                retVal.AddError("Failed to start the Plugin: " + ex.Message);
            }

            if (retVal.ResultCode != ResultCode.Failure)
                ChangeState(State.Running);
            else
                ChangeState(State.Faulted, retVal.GetLastError());

            retVal.LogResult(logger);
            logger.ExitMethod(retVal, guid);
            return retVal;
        }

        public Result Stop(StopType stopType = StopType.Stop)
        {
            Guid guid = logger.EnterMethod(true);

            Result retVal = new Result();
            ChangeState(State.Stopping);

            try
            {
                // todo: implement shutdown logic
            }
            catch (Exception ex)
            {
                retVal.AddError("Failed to stop the Plugin: " + ex.Message);
            }

            if (retVal.ResultCode != ResultCode.Failure)
                ChangeState(State.Stopped);
            else
                ChangeState(State.Faulted);

            retVal.LogResult(logger);
            logger.ExitMethod(retVal, guid);
            return retVal;
        }

        #endregion Public Methods

        #region Private Methods

        private void ChangeState(State state, string message = "")
        {
            State previousState = State;

            State = state;

            if (StateChanged != null)
                StateChanged(this, new StateChangedEventArgs(state, previousState, message));
        }

        #endregion Private Methods
    }
}