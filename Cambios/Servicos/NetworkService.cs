﻿using Cambios.Modelos;
using System.Net;

namespace Cambios.Servicos
{
    public class NetworkService
    {
        public Response CherckConnection()
        {
            var client = new WebClient();

            try
            {
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return new Response
                    {
                        IsSuccess = true,
                    };
                }
            }
            catch 
            {

                return new Response
                {
                    IsSuccess = false,
                    Message = "Configure a sua ligação à internet",
                };
            }
        }
    }
}
