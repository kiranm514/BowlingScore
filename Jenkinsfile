pipeline {
    agent {label 'agent2'}

    stages {
        stage('Build') {
            steps {
                echo 'Building..'
            }
        }
        stage('promte') {
            steps {
                echo 'promoting..'
            }
        }
        stage('Test') {
            steps {
                echo 'Testing..'
            }
        }
        stage('Deploy') {
            steps {
                echo 'Deploying....'
            }
        }
    }
}
