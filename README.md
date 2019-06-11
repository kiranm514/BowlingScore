# BowlingScore
Calculate Bowling Score using C# and AWS lambda functions.
When individual Frame score file is uploaded to AWS S3 bucket, S3 event is triggred to run Lambda function and claculate the score and copy to the file. then the file is uploaded to S3 bucket.
