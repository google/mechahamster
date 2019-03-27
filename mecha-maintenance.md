# Overview

The MechaHamster cloud setup has several moving pieces which make it work.
These pieces will be described in enough detail that will hopefully allow for
their maintenance by thrid parties. Note that this does not exhaustively
describe all the services and deployments running to make the MechaHamster
server work; just the relevant pieces of the puzzle are:

* The MechaHamster Lobby Server
* The OpenMatch Front End
* The OpenMatch Director
* The OpenMatch MMF Reaper
* The Agones Controller
* The MechaHamster Match Server
* The MechaHamster Load Simulator

## Prerequisites

In order to work on this project it is assumed that one has access to the
"Mecha Hamster" project on GCP, has the glcoud tools installed for the
platform and are properly credentialled to execute commands locally on VMs.

## Game Flow

When a client starts, it begins by joining the _MechaHamster Lobby Server_.
This is a pre-match game which simply waits for 4 clients to arrive before
instructing them to request an OpenMatch match. Once four clients arrive in
the lobby, each client contacts the OpenMatch Front End and awaits the
IP/Port of a MechaHamster Match Server to which to join. When OpenMatch is
contacted via the _OpenMatch Front End_, a series of server-side events are
kicked off which ultimately results in Agones allocating a
_MechaHamster Match Server_ to which the clients can join. After a match has
ended and all clients disconnect from the match server, the server instructs
Agones that it should be shutdown. This is different from the lobby server,
which remains running indefinitely; the match servers have a lifespan of a
single game.

## The MechaHamster Lobby Server

The _MechaHamster Lobby Server_ is a single instance of the MechaHamster
server running on an isolated VM with an exposed public IP address and
corresponding firewall ingress rule. It is _not_ running inside the GKE
cluster.

| Information | Details |
| --- | --- |
| VM Name | mecha-test-vm |
| Public IP | 35.236.114.54 |
| Public Port | 7777/UDP |
| Container Image | gcr.io/mechahamster/mechahamster:lobby-latest |
| Firewall Rule | mecha-test-vm-ingress |

To run the server it is necessary to ssh into the VM and run the Docker
container image manually. The Docker image is run exposing the server port
with the following command:

```
docker run -d -p 7777:7777/udp --name mecha -t gcr.io/mechahamster/mechahamster:lobby-latest
```

### Rolling the Lobby Server

The process is manual, but simple. Stop and remove the old container
instance, and start a new one.

```
docker stop mecha
docker rm mecha
docker run -d -p 7777:7777/udp --name mecha -t gcr.io/mechahamster/mechahamster:lobby-latest
```

## The OpenMatch Front End

The OpenMatch deployment is alltogether mode complex than the other pieces of
the server configuration due to the early stage nature of the OpenMatch
release. The Front End is largely a hands-off piece, however, but it is
important to understand that there must be an ingress here from the game
client to access OpenMatch. The ingress is provided by an GKE load balancer
which forwards traffic to the service.

| Information | Details |
| --- | --- |
| Public IP | 35.236.24.200 |
| Public Port | 50504/TCP |
| Service Name | om-frontendapi |
| Load Balancer | om-api-ingress |

## The OpenMatch Director

The director is a service that proxies game server allocation to Agones when
a match is made. The director runs as a Kubernetes deployment and sits
internal to the cluster. The director coordinates with the
_OpenMatch Back End_, triggers the execution of Match Making Functions (MMFs),
and loads and stores data to Redis.

The director is configured to run two match rules; one which matches game
clients together which allows physical play, the other as a match
demonstration so that the _MechaHamster Load Simulator_ can allocate servers
without interfering with the game.

The MMFs run as Kubernetes pods which complete once they have decided if a
match exists. This can be seen in the GKE Workloads console as a large number
of finished jobs that end in ".mmf" or ".evaluator".

| Information | Details |
| --- | --- |
| Container Image | gcr.io/mechahamster/openmatch-director:dev |
| Service Name | openmatch-director |
| Game Match MMF Name | somethingv1 |
| Game Match Criteria | battleroyale: 1 |
| Load Simulator MMF Name | demov1 |
| Load Simulator Match Criteria | demo: 1 |

It is worth noting that MMF jobs are prepended with their name, so there
should always be jobs run as "\<identifier\>.somethingv1.mmf" and
"\<identifier\>.demov1.mmf".

### Running the Director

In the MechaHamster repository there is a Kubernetes YAML configuration which
is used to run this file: `mechahamster-director.yaml`.

Its internals are repeated here for informational purposes:

```
apiVersion: apps/v1
kind: Deployment
metadata:
  name: openmatch-director
  labels:
    app: openmatch
    component: director
spec:
  replicas: 1
  selector:
    matchLabels:
      app: openmatch
      component: director
  template:
    metadata:
      labels:
        app: openmatch
        component: director
    spec:
      containers:
      - name: openmatch-director
        image: gcr.io/mechahamster/openmatch-director:dev
        imagePullPolicy: Always
```

## The OpenMatch MMF Reaper

The current early version of OpenMatch spawns MMF jobs at a high frequency.
The job pods remain without being cleaned up in order to assist with debugging
such early software. Currently there is no setting in OpenMatch to enable
cleanup. Therefore, this service exists as a Kubernetes cron job to
periodically clean up jobs tagged as "mmf". It serves no foundational purpose
and exists only to keep clutter managable.

Should the reaper service ever disappear, it can be started with from a
Kubernetes configuration YAML that exists in the MechaHamster repository:
`mechahamster-reaper.yaml`.

Its internals are repeated here for informational purposes:

```
apiVersion: batch/v1beta1
kind: CronJob
metadata:
  name: k8s-job-cleaner
  labels:
    job: k8s-job-cleaner
    role: job
spec:
  schedule: "*/1 * * * *"
  concurrencyPolicy: Allow
  suspend: false
  jobTemplate:
    metadata:
      name: k8s-job-cleaner
      labels:
        job: k8s-job-cleaner
        role: job
    spec:
      template:
        metadata:
          name: k8s-job-cleaner
          labels:
            job: k8s-job-cleaner
            role: job
        spec:
          containers:
          - name: k8s-job-cleaner
            image: quay.io/dtan4/k8s-job-cleaner:latest
            imagePullPolicy: Always
            command:
              - "/k8s-job-cleaner"
              - "--in-cluster"
              - "--label-group"
              - "app"
              - "--max-count"
              - "4"
          restartPolicy: Never

```

## The Agones Controller

The _Agones Controller_ is the overseer of all the complexity that is well
hidden behind Agones. The controller itself is largely maintenance free, but
the fleet of gameservers behind it occasionally require babysitting.

Agones allocates MechaHamster Match Servers from a fleet of available servers.
The fleet auto-scaler keeps a live count of warm servers ready to service
clients and automatically increases and reduces that number based on the
configuration applied.

| Information | Details |
| --- | --- |
| Firewall Rule | gke-mecha-hamster-cluster-36af99c4-agones |
| GCP Target Tag | game-server |

The fleet and auto-scaler configuration is applied from a Kubernetes YAML
configuration file in the MechaHamster repository: `mechahamster-agones.yaml`.

Its internals are repeated here for informational purposes:

```
apiVersion: "stable.agones.dev/v1alpha1"
kind: Fleet
metadata:
  name: mecha-hamster
spec:
  replicas: 1
  template:
    spec:
      ports:
      - name: default
        containerPort: 7777
      template:
        spec:
          containers:
          - name: mecha-hamster
            image: gcr.io/mechahamster/mechahamster:agones-latest
            imagePullPolicy: Always
            resources:
              limits:
                cpu: "2"
              requests:
                cpu: "1"
---
apiVersion: "stable.agones.dev/v1alpha1"
kind: FleetAutoscaler
metadata:
  name: mecha-hamster-autoscaler
spec:
  fleetName: mecha-hamster
  policy:
    type: Buffer
    buffer:
      bufferSize: 4
      minReplicas: 4
      maxReplicas: 20
```

### Rolling the GameServers

If it becomes necessary to delete all the game servers, the following command
line will help:

`kubectl get gs | grep mecha | awk '{ print $1 }' | xargs kubectl delete gs`

Keep in mind, unless the FleetAutoscaler is disabled, the servers will be
recreated.

## The MechaHamster Match Server

The match server is the same as the lobby server with a slightly different
codepath enabled so that it can interface with Agones. This codepath is
selected via command-line arguments baked into the Docker container. The
containers are run automagically by the Fleet and maintained live until
players enter and exit them.

While in operation, these servers, as Agones GameServer objects, maintain a
Ready state awaiting allocation via OpenMatch. Until a client connection is
made, the server sits in a waiting state and periodically (once ever 30
seconds) issues a Ready command to Agones. This is heavy-handed fix to the
rare bug where a server is allocated, but the clients crash or are otherwise
unable to connect to it; the server will return itself to the Ready pool if
nobody is in it within 30 seconds. This also has the happy side effect of
allowing the Load Simulator tool to operate without requiring actual Unity
clients (and avoid leaking servers).

| Information | Details |
| --- | --- |
| Container Image | gcr.io/mechahamster/mechahamster:agones-latest |

## The MechaHamster Load Simulator

The MechaHamster Load Simulator makes the Agones Grafana dashboard pretty. It
is a service which sends a bogus match request periodically to OpenMatch
without actually connecting to the game servers. It relies on the server
timeout to reset the servers back to ready.

The current mode of operation for the simulator is to spin up 4 rounds of
games roughly 10 seconds apart and wait 30 seconds for the timeout to reset
the servers, once a reply from OpenMatch happens to say a match exists. The
program allocates from a pool of 4 matches and will not allocate more until
the timeouts expire. It operates in bursts of 3 minute intervals followed by
2 minutes of waiting. This gives the graphs some depth as it shows blocks of
allocations happening followed by silence.

The tool is run as a Kubernetes deployment, however the source is not yet
committed to the repository due to layout differences between how Unity
expects a project and how Go expects a project.

The Kubernetes configuration YAML is repeated here:

```
apiVersion: apps/v1
kind: Deployment
metadata:
  name: mecha-load-test
  labels:
    app: mecha-load-test
    component: loadtest
spec:
  replicas: 1
  selector:
    matchLabels:
      app: mecha-load-test
      component: loadtest
  template:
    metadata:
      labels:
        app: mecha-load-test
        component: loadtest
    spec:
      containers:
      - name: mecha-load-test
        image: gcr.io/mechahamster/mecha-load-test:latest
        imagePullPolicy: Always
```
