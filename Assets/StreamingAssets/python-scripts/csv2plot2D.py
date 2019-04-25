# %% Import packages

# uncomment if using cells

import pandas as pd
import argparse
import os
from scipy import stats
import numpy as np


def parse_arguments():
    parser = argparse.ArgumentParser(description='Process some integers.')
    parser.add_argument('-f', '--filename', type=str, help='The csv input', required=True)
    parser.add_argument('-d', '--delimiter', default=',', type=str, help='the csv delimiter')
    parser.add_argument('-c', '--columns', nargs='+', required=True, help='The names of the two columns')
    parser.add_argument('-s', '--scatterplot', action='store_true', help='Set to create scatter plot.')
    parser.add_argument('-l', '--line-plot-mean', action='store_true', help='Creates a line plot by '
                                                                            'using mean on the first column')
    parser.add_argument('-b', '--bar-plot-mean', action='store_true', help='Creates a bar plot by '
                                                                           'using mean on the first column')
    parser.add_argument('--show-figs', action='store_true', help='Set to display figs')
    parser.add_argument('--rm-outliers', action='store_true', help='Removes outliers in mean plots')
    parser.add_argument('--store-dataframe-description', action='store_true', help='Saves dataframe description in a '
                                                                                   'file')

    return parser.parse_args()


# %% This Cell does nothing when testingwith cells
def main():
    args = parse_arguments()

    filename = args.filename
    delim = args.delimiter
    cols = args.columns

    scatterplot = args.scatterplot
    line_plot_mean = args.line_plot_mean
    bar_plot_mean = args.bar_plot_mean

    show_figs = args.show_figs
    rm_outliers = args.rm_outliers
    store_dataframe_description = args.store_dataframe_description

    if not scatterplot and not line_plot_mean and not bar_plot_mean:
        scatterplot = True

    # %% Read csv

    #  ## Uncomment for testing if using cells ##

    # filename = 'C:\Projects\sumo-in-unity\Assets\StreamingAssets\python-scripts\sim_step_length-by_num_veh40.txt'
    # delim = ','
    # cols = ['vehicle_count', 'sim_step_delay']
    # scatterplot = True
    # line_plot_mean = True
    # bar_plot_mean = True
    # show_figs = True
    # rm_outliers = True
    # store_dataframe_description = True

    filename_no_extension = os.path.splitext(filename)[0]
    print('Filename                 : ' + filename)
    print('Filename no extension    : ' + filename_no_extension)
    print('Delimiter                : ' + delim)
    print('Columns                  : ' + str(cols))
    print('Remove outliers          : ' + str(rm_outliers))
    print('Create Scatterplot       : ' + str(scatterplot))
    print('Create means line plot   : ' + str(line_plot_mean))
    print('Create means bar plot    : ' + str(bar_plot_mean))
    print('Store dataframe description : ' + str(store_dataframe_description))
    print('Show figures             : ' + str(show_figs))

    names = [cols[0], cols[1]]
    df = pd.read_csv(filepath_or_buffer=filename, header=3, delimiter=delim, names=names, engine='python')
    df_only_outliers = None
    df_raw = df.copy()

    if rm_outliers:
        df_only_outliers = df[df[cols[1]] >= 600]
        df = df[df[cols[1]] < 500]
        #   df = df[(np.abs(stats.zscore(df)) < 3).all(axis=1)]

    # %% create line plot mean
    if line_plot_mean or bar_plot_mean:
        # %% group by first column
        df_mean_by_first_col = df.groupby([cols[0]]).mean()

        # %% line plot
        if line_plot_mean:
            plot = df_mean_by_first_col.plot(kind='line')
            fig = plot.get_figure()
            means_line_filename = filename_no_extension + '-means-line'
            fig.savefig(means_line_filename + '.png', bbox_inches='tight', format='png')
            fig.savefig(means_line_filename + '.pdf', bbox_inches='tight', format='pdf')
            if show_figs:
                fig.show()
            print("Absolute output path : " + os.path.abspath(means_line_filename))
            print("Created plot png: " + means_line_filename + '.png')
            print("Created plot pdf: " + means_line_filename + '.pdf')

        # %% bar plot
        if bar_plot_mean:
            plot = df_mean_by_first_col.plot(kind='bar', figsize=(60, 10))
            fig = plot.get_figure()
            means_bar_filename = filename_no_extension + '-means-bar'
            fig.savefig(means_bar_filename + '.png', bbox_inches='tight', format='png')
            fig.savefig(means_bar_filename + '.pdf', bbox_inches='tight', format='pdf')
            if show_figs:
                fig.show()
            print("Created plot png: " + means_bar_filename + '.png')
            print("Created plot png: " + means_bar_filename + '.pdf')

    # %% scatter plot
    if scatterplot:
        plot = df.plot.scatter(x=cols[0], y=cols[1])
        fig = plot.get_figure()

        means_scatter_filename = filename_no_extension + '-scatter'
        fig.savefig(means_scatter_filename + '.png', bbox_inches='tight', format='png')
        fig.savefig(means_scatter_filename + '.pdf', bbox_inches='tight', format='pdf')
        if show_figs:
            fig.show()
        print("Created plot png: " + means_scatter_filename + '.png')
        print("Created plot pdf: " + means_scatter_filename + '.pdf')

    if store_dataframe_description:
        description_filename = filename_no_extension + "-description.txt"
        f = open(description_filename, "w+")
        f.write("Raw dataframe description:\n" + str(df_raw.describe()) + "\n")
        if rm_outliers:
            f.write("dataframe with no outliers description :\n" + str(df.describe()) + "\n")

        if rm_outliers:
            f.write("Only outliers dataframe:\n" + str(df_only_outliers.values))

        f.close()
        print("Created description file " + description_filename)


# %% main Do NOT execute when using cells
if __name__ == '__main__':
    main()
