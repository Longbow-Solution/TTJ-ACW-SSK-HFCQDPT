﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ACWSSK.ACWWS {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ACWWS.IACWWS")]
    public interface IACWWS {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IACWWS/GetLastTx", ReplyAction="http://tempuri.org/IACWWS/GetLastTxResponse")]
        string[] GetLastTx(string[] tables, string componentCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IACWWS/GetLastTx", ReplyAction="http://tempuri.org/IACWWS/GetLastTxResponse")]
        System.Threading.Tasks.Task<string[]> GetLastTxAsync(string[] tables, string componentCode);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IACWWS/ValidateUser", ReplyAction="http://tempuri.org/IACWWS/ValidateUserResponse")]
        bool ValidateUser(string username, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IACWWS/ValidateUser", ReplyAction="http://tempuri.org/IACWWS/ValidateUserResponse")]
        System.Threading.Tasks.Task<bool> ValidateUserAsync(string username, string password);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IACWWSChannel : ACWSSK.ACWWS.IACWWS, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ACWWSClient : System.ServiceModel.ClientBase<ACWSSK.ACWWS.IACWWS>, ACWSSK.ACWWS.IACWWS {
        
        public ACWWSClient() {
        }
        
        public ACWWSClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ACWWSClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ACWWSClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ACWWSClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public string[] GetLastTx(string[] tables, string componentCode) {
            return base.Channel.GetLastTx(tables, componentCode);
        }
        
        public System.Threading.Tasks.Task<string[]> GetLastTxAsync(string[] tables, string componentCode) {
            return base.Channel.GetLastTxAsync(tables, componentCode);
        }
        
        public bool ValidateUser(string username, string password) {
            return base.Channel.ValidateUser(username, password);
        }
        
        public System.Threading.Tasks.Task<bool> ValidateUserAsync(string username, string password) {
            return base.Channel.ValidateUserAsync(username, password);
        }
    }
}
