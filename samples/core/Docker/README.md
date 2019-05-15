This project was created on macOS  Mojave (10.14.2)
using VSCode version 1.31, docker runtime 18.09.2

To build and run container please start

docker-compose up

in root folder of current project.
Inside container (name samplesrv) there will be SampleService application
based on Kestrel server with default Version controller.

Port mapping is used: 10001 -> 10001

Please provide SEQ url for logging (appsettings.json) if you would like to use SEQ log storage.

Auto redirect localhost:10001 should be applied to localhost:10001/index.html (swagger endpoint)