﻿using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Class that represents a authorization token for the server
/// <author>Mikael Nilssen</author>
/// </summary>

namespace HardwareCommunicator {
    public class AccessToken {

        //Intended for use when connecting to remote server. Generate on website and put manually in app settings
        public string connectionHash { get; private set; }

        public AccessToken(string connectionHash) {
            this.connectionHash = connectionHash;
        }

        internal string getAccessString() {
            return connectionHash;
        }
    }
}
