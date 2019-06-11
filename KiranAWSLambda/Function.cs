using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace KiranAWSLambda
{
    public class Function
    {
        IAmazonS3 S3Client { get; set; }

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public Function()
        {
            S3Client = new AmazonS3Client();
        }

        /// <summary>
        /// Constructs an instance with a preconfigured S3 client. This can be used for testing the outside of the Lambda environment.
        /// </summary>
        /// <param name="s3Client"></param>
        public Function(IAmazonS3 s3Client)
        {
            this.S3Client = s3Client;
        }

        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
        /// to respond to S3 notifications.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public int findFallenPins(string s1, string s2, string s3)
        {

            int pinsfallen = 0;            
                if(!string.IsNullOrWhiteSpace(s3))
                {
                    pinsfallen = Convert.ToInt32(s1)+Convert.ToInt32(s2)+Convert.ToInt32(s3);
                }
            else
            {
                pinsfallen = Convert.ToInt32(s1) + Convert.ToInt32(s2) ;
            }

            
            if (pinsfallen >= 10)
            {
                pinsfallen = 10;
            }
            return pinsfallen;
        }

        public async Task<String> FunctionHandler(S3Event evnt, ILambdaContext context)
        {
            int counter = 0;
            string responseBody = "";
            var s3Event = evnt.Records?[0].S3;
            context.Logger.LogLine($"event:{s3Event.Bucket.Name}");
            if (s3Event == null)
            {
                return "No Events";
            }

            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = s3Event.Bucket.Name,
                    Key = s3Event.Object.Key
                };

                using (GetObjectResponse response = await S3Client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string output = "";                    
                    responseBody = reader.ReadToEnd(); // Now you process the response body.
                    string[] lines = responseBody.Split('\n');

                    Dictionary<string, dynamic> scoresList = new Dictionary<string, dynamic>();

                    if (lines.Length == 0)
                    {
                        throw new InvalidOperationException("The file is empty");
                    }                    
                    var rows = lines.Select(l => l.Split(',').ToArray()).ToArray();                    
                    for (int i=1; i<lines.Length;i++)
                    {                                              
                        int firstpins=0, secondpins=0, thirdpins = 0;
                        firstpins = findFallenPins(rows[i][1], rows[i][2], rows[i][3]);                        
                        if (i<(lines.Length - 1))
                        {                            
                            secondpins = findFallenPins(rows[i+1][1], rows[i+1][2], rows[i+1][3]);
                        }
                        if (i<(lines.Length - 2))
                        {
                          
                            thirdpins = findFallenPins(rows[i+2][1], rows[i+2][2], rows[i+2][3]);
                           
                        }
                        
                        int score = 0;
                        
                        if (string.IsNullOrWhiteSpace(rows[i][3]))
                            {
                            if (i < (lines.Length - 2))
                            {
                                
                                if (Convert.ToInt32(rows[i][1]) == 10)
                                {

                                    if (rows[i][0] == rows[i+2][0])
                                    {
                                        if (Convert.ToInt32(rows[i+1][1])==10)
                                        {
                                            score = 10 + 10 + Convert.ToInt32(rows[i+2][1]);
                                        }
                                        else if(Convert.ToInt32(rows[i+1][1]) < 10)
                                        {
                                            score = 10 + secondpins;
                                        }
                                    }
                                    else
                                    {
                                        score = 10 + Convert.ToInt32(rows[i + 1][1]) + Convert.ToInt32(rows[i + 1][2]);
                                    }

                                }
                                else if (firstpins == 10)
                                {
                                    score = firstpins + Convert.ToInt32(rows[i + 1][1]);
                                }
                                else
                                {
                                    score = firstpins;
                                }
                            }
                            else if(i==(lines.Length- 2))
                            {
                                if (Convert.ToInt32(rows[i][1]) == 10)
                                {     
                                     
                                        score = 10 + Convert.ToInt32(rows[i + 1][1]) + Convert.ToInt32(rows[i + 1][2]);                                   

                                }
                                else if (firstpins == 10)
                                {
                                    score = 10 + Convert.ToInt32(rows[i + 1][1]);
                                }
                                else
                                {
                                    score = firstpins;
                                }
                            }
                            else if (i == (lines.Length-1))
                            {
                                score = firstpins;
                            }
                            }
                            else if (!string.IsNullOrWhiteSpace(rows[i][3]))
                            {
                                score = Convert.ToInt32(rows[i][1]) + Convert.ToInt32(rows[i][2]) + Convert.ToInt32(rows[i][3]);
                            }
                        context.Logger.LogLine($"score:{score}");
                        
                        dynamic val;
                        if (scoresList.TryGetValue(rows[i][0], out val))
                        {
                            context.Logger.LogLine($"Counter Value:{counter}");
                            counter++;
                            scoresList[rows[i][0]] = val + score;
                            if (counter>=10)
                            {
                                context.Logger.LogLine($"Error:more than 10 games played");
                                scoresList[rows[i][0]] = "Error:more than 10 games played";
                            }
                            context.Logger.LogLine($"acore added:");
                        }
                        else
                        {
                            if (counter == 9 || counter == 0)
                            {
                                scoresList.Add(rows[i][0], score);
                                
                            }
                            else
                            {
                                context.Logger.LogLine($"Error:Less than 10 games played");
                                scoresList[rows[i - 1][0]] = "Error:Less than 10 games played";
                                scoresList.Add(rows[i][0], score);
                            }
                            counter = 0;
                        }
                    }
                    foreach (KeyValuePair<string,dynamic> kvp in scoresList)
                    {
                        output += kvp.Key + ',' + kvp.Value + '\n';
                                                
                    }

                    //context.Logger.LogLine($"{output}");
                    PutObjectRequest request1 = new PutObjectRequest
                    {
                        BucketName = s3Event.Bucket.Name,
                        Key = "scores.csv",
                        ContentBody = output
                    };
                    
                    var result = await S3Client.PutObjectAsync(request1);
                    
                    context.Logger.LogLine($"{output}");                   
                    context.Logger.LogLine($"end");
                    return "Sucess";
                }

                
            }
            catch (Exception e)
            {
                //context.Logger.LogLine($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
                context.Logger.LogLine(e.Message);
                context.Logger.LogLine(e.StackTrace);
                throw;
            }
        }
    }
}
