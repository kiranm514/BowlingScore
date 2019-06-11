using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.S3Events;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;

using KiranAWSLambda;

namespace KiranAWSLambda.Tests
{
    public class FunctionTest
    {
        [Fact]
        public async Task TestS3EventLambdaFunction()
        {
           // IAmazonS3 s3Client = new AmazonS3Client(RegionEndpoint.USWest2);

            var bucketName = "lambda-KiranAWSLambda-".ToLower() + DateTime.Now.Ticks;
            var key = "text.txt";

            // Create a bucket an object to setup a test data.
            //await s3Client.PutBucketAsync(bucketName);
            try
            {
                /*await s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = key,
                    ContentBody = "sample test"
                    //"player,ball_1,ball_2,ball_3 Bryan,8,2,0 Bryan,3,0,0 Bryan,10,0,0 Bryan,10,0,0 Bryan,0,0,0 Bryan,9,1,0 Bryan,5,5,0 Bryan,10,0,0 Bryan,10,0,0 Bryan,10,10,10"
                });*/

                // Setup the S3 event object that S3 notifications would create with the fields used by the Lambda function.
                var s3Event = new S3Event
                {
                    Records = new List<S3EventNotification.S3EventNotificationRecord>
                    {
                        new S3EventNotification.S3EventNotificationRecord
                        {
                            S3 = new S3EventNotification.S3Entity
                            {
                                Bucket = new S3EventNotification.S3BucketEntity {Name = bucketName },
                                Object = new S3EventNotification.S3ObjectEntity {Key = key }
                           
                            }
                        }
                    }
                };

                // Invoke the lambda function and confirm.
                var function = new Function(s3Client);
                var output = await function.FunctionHandler(s3Event, null);

                Assert.Equal("No Events", output);

            }
            finally
            {
                // Clean up the test data
                await AmazonS3Util.DeleteS3BucketWithObjectsAsync(s3Client, bucketName);
            }
        }
    }
}
