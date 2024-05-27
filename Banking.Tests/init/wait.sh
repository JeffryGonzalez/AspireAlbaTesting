#!/bin/bash

until [ pg_isready ] ; do sleep 5 ; done