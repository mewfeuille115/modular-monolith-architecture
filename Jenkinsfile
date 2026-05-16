pipeline {
    agent any

    environment {
        SONAR_SERVER       = 'sonarqube-server'
        REPO_NAME          = "${env.GIT_URL.split('/').last().split('\\.').first()}"
        DOTNET_SDK_IMAGE   = 'mcr.microsoft.com/dotnet/sdk:10.0'
        SOLUTION           = 'Evently.slnx'
        SONAR_ORG          = credentials('sonar-org')
        CONTAINER_REGISTRY = credentials('container-registry')
        IMAGE_NAME         = "${env.CONTAINER_REGISTRY}/evently-api"
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

        stage('Docker Build') {
            steps {
                sh '''
                    cp Directory.Build.props Directory.Packages.props .editorconfig src/
                    docker build \
                        -t $IMAGE_NAME:$BUILD_NUMBER \
                        -t $IMAGE_NAME:latest \
                        -f src/API/Evently.Api/Dockerfile ./src
                '''
            }
        }

        stage('Publish Docker Image') {
            steps {
                sh '''
                    gcloud auth print-access-token \
                        | docker login -u oauth2accesstoken --password-stdin us-central1-docker.pkg.dev
                    docker push $IMAGE_NAME:$BUILD_NUMBER
                    docker push $IMAGE_NAME:latest
                '''
            }
        }

    }

    post {

        failure {
            echo 'El pipeline falló. Revisá los logs.'
        }

        always {
            node('') {
                sh 'docker rmi $IMAGE_NAME:$BUILD_NUMBER $IMAGE_NAME:latest || true'
            }
            cleanWs()
        }

    }
}
