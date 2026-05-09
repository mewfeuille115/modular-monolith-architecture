pipeline {
    agent any

    environment {
        SONAR_SERVER     = 'sonarqube-server'
        REPO_NAME        = "${env.GIT_URL.split('/').last().split('\\.').first()}"
        DOTNET_SDK_IMAGE = 'mcr.microsoft.com/dotnet/sdk:10.0'
        SOLUTION         = 'Evently.slnx'
        PUBLISH_PROJECT  = 'src/API/Evently.Api/Evently.Api.csproj'
        SONAR_ORG        = credentials('sonar-org')
    }

    stages {

        stage('Get Source') {
            steps {
                checkout scm
                echo "Repositorio: ${env.REPO_NAME}"
            }
        }

        stage('Build & QA') {
            parallel {

                stage('Build') {
                    agent {
                        docker {
                            image "${env.DOTNET_SDK_IMAGE}"
                            args  '-u root'
                            reuseNode true
                        }
                    }
                    steps {
                        sh "dotnet build ${env.SOLUTION} --configuration Release"
                    }
                }

                stage('Análisis SonarQube') {
                    agent {
                        docker {
                            image "${env.DOTNET_SDK_IMAGE}"
                            args  '-u root'
                            reuseNode true
                        }
                    }
                    steps {
                        withSonarQubeEnv("${env.SONAR_SERVER}") {
                            sh """
                                dotnet tool install --global dotnet-sonarscanner
                                export PATH="\$PATH:\$HOME/.dotnet/tools"

                                dotnet sonarscanner begin \\
                                    /k:"${env.REPO_NAME}" \\
                                    /n:"${env.REPO_NAME}" \\
                                    /o:"${env.SONAR_ORG}" \\
                                    /d:sonar.host.url="\${SONAR_HOST_URL}" \\
                                    /d:sonar.token="\${SONAR_AUTH_TOKEN}"

                                dotnet build ${env.SOLUTION} --configuration Release

                                dotnet sonarscanner end \\
                                    /d:sonar.token="\${SONAR_AUTH_TOKEN}"
                            """
                        }
                    }
                }

            }
        }

        stage('Quality Gate') {
            steps {
                catchError(buildResult: 'UNSTABLE', stageResult: 'UNSTABLE') {
                    waitForQualityGate abortPipeline: false
                }
            }
        }

        stage('Publish Artifact') {
            agent {
                docker {
                    image "${env.DOTNET_SDK_IMAGE}"
                    args  '-u root'
                    reuseNode true
                }
            }
            steps {
                sh """
                    dotnet publish ${env.PUBLISH_PROJECT} \\
                        --configuration Release \\
                        --output ./artifacts
                """
                archiveArtifacts artifacts: 'artifacts/**', fingerprint: true
            }
        }

    }

    post {
        failure {
            echo 'El pipeline falló. Revisá los logs del Quality Gate o del análisis SonarQube.'
        }
        always {
            cleanWs()
        }
    }
}
