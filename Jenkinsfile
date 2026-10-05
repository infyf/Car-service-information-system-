pipeline {
    agent any

    stages {
        stage('Restore & Build Backend') {
            steps {
                dir('backend') {
                    sh 'dotnet restore'
                    sh 'dotnet build -c Release'
                }
            }
        }

        stage('Build Frontend') {
            steps {
                dir('frontend') {
                    sh 'npm install'
                    sh 'npm run build'
                }
            }
        }
    }
}
